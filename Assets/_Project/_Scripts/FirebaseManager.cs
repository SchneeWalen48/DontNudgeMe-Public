using Firebase;
using Firebase.Auth;
using Firebase.Database;
using Google.MiniJSON;
using Photon.Pun;
using Photon.Realtime;
using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;
using WebSocketSharp;

public class FirebaseManager : MonoBehaviour
{
    #region Defining Singleton
    public static FirebaseManager Instance { get; private set; }
    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);


    }
    #endregion
   
    public FirebaseApp App { get; private set; }
    public FirebaseAuth Auth { get; private set; }
    public FirebaseDatabase DB { get; private set; }


    private DatabaseReference CurrentUserDataRef => DB.RootReference.Child($"Users/{Auth.CurrentUser.UserId}/Data");
    public DatabaseReference CurrentUserCustomizationDataRef => DB.RootReference.Child($"Users/{Auth.CurrentUser.UserId}/CustomizationData");

    private DatabaseReference AllNamesRef => DB.RootReference.Child($"ReservedNames");

    //TODO: 1. DB와 연동되는 UserData 클래스 정의, 활용
    //[SerializeField] private UserData currentUserData;
    //private DatabaseReference messageRef;


    //void OnLogin()
    //{
    //    messageRef = DB.GetReference($"msg/{Auth.CurrentUser.UserId}");
    //    messageRef.ChildAdded += OnMessageReceived;
    //}

    //private void OnMessageReceived(object sender, ChildChangedEventArgs e)
    //{
    //    if (e.DatabaseError == null) //에러가 없으면
    //    {
    //        string message = e.Snapshot.GetValue(true).ToString();

    //        Debug.Log(message);
    //    }
    //}




    async void Start()
    {
        //파이어베이스 앱을 초기화해야 함. 파이어베이스의 라이브러리는 TPL을 사용하는 참조가 많으므로
        //async 키워드를 붙여 비동기 함수로 만듦.

        if (UserData.Local != null)
            UserData.Local = null;
        DependencyStatus status = await FirebaseApp.CheckAndFixDependenciesAsync(); //파이어베이스가 유효한 상태인지 체크
        var options = new AppOptions()
        {
            ApiKey = "",
            DatabaseUrl = new(""),
            ProjectId = "",
            StorageBucket = "",
            MessageSenderId = "",
            AppId = ""
        };

        App = FirebaseApp.Create(options);


        if (status == DependencyStatus.Available)
        {

            //초기화 성공
            Debug.Log($"파이어베이스 초기화 성공");
            Auth = FirebaseAuth.GetAuth(App);
            DB = FirebaseDatabase.GetInstance(App);

            //이제 DB에서 유저의 데이터를 가져오는 처리
            //DatabaseReference rootRef = DB.RootReference;
            //Debug.Log(rootRef);
            //DatabaseReference testRef = rootRef.Child("UsersTest"); //TODO: 테스트용 UsersTest임. 실제 운영될 데이터베이스는 이름 다르게.
            //Debug.Log(testRef);
            

            ////데이터베이스에서 데이터 조회 성공
            //Debug.Log($"dummy Data: {snapshot.Value}");


            //snapshot = await rootRef.Child("test/dummyNumber").GetValueAsync();
            //Debug.Log($"dummy Number: {snapshot.Value}");
        }
        else
        {
            //초기화 실패
            Debug.LogWarning($"파이어베이스 초기화 실패, 파이어베이스 앱 상태: {status}");
        }
    }



    //회원가입 함수: 비동기함수이며, 콜백을 받을 예정임.
    public async void CreateAccount(string email, string password, Action<FirebaseUser> callback = null)
    {
        try
        {
            AuthResult result = await Auth.CreateUserWithEmailAndPasswordAsync(email, password);
            //여기서 exception이 발생하지 않으면 회원가입에 성공했다는 의미.
            //만약 exception이 발생하면 아래의 catch로 넘어감.
            Debug.Log("회원가입 성공");
            callback?.Invoke(result.User);
            string uid = result.User.UserId;
            //Test only (DB root/UsersTest에 uid값 집어넣어보기)
            DatabaseReference createdRef = DB.RootReference.Child("Users").Child(uid);
            await createdRef.SetValueAsync(uid);

            UserData newUser = new UserData();
            UserProfile profile = new() { DisplayName = newUser.userName };
            string json = JsonUtility.ToJson(newUser);
            DatabaseReference userDataRef = createdRef.Child("Data");
            await userDataRef.SetRawJsonValueAsync(json);
            await result.User.UpdateUserProfileAsync(profile);

            //HACK: 강욱-0928
            //계정을 생성할 때에 커스터마이징 정보 참조에 새로운 커스터마이징 정보 만들어주기
            DatabaseReference customizationDataRef = createdRef.Child("CustomizationData");
            await customizationDataRef.SetRawJsonValueAsync(JsonUtility.ToJson(new CustomizationData()));


            /*//TODO: 1. DB와 연동되는 UserData 클래스 정의, 활용 후 주석 해제
            UserData userData = new UserData
            {
                name = "defaultName",
                job = "Novice",
                level = 1,
                exp = 0f,
                pos = Vector2.one,
                dead = false
            };
            string json = JsonUtility.ToJson(userData);

            userRef = DB.RootReference.Child($"users/{result.User.UserId}");
            await userRef.SetRawJsonValueAsync(json);*/

        }
        catch (FirebaseException fe)
        {
            Debug.LogError($"파이어베이스 에러: {fe.Message}");
            callback?.Invoke(null);
        }

    }

    public async Task UpdateUserData(UserData data)
    {
        try
        {
            string json = JsonUtility.ToJson(data);
            await CurrentUserDataRef.SetRawJsonValueAsync(json);
        }
        catch (FirebaseException fe)
        {
            Debug.Log($"파이어베이스 에러: {fe.Message}");
        }
    }

    public async Task<bool> ChangeUserTitle(string newTitle, Action<string> displayMessage = null)
    {
        //가장 먼저 엣지케이스 처리: 쓸 수 없는 닉네임은 쳐내기
        //TODO: 나중에 정규식 넣어서 필터링할수도 있겠음
        if (newTitle == "DEFAULT")
        {
            displayMessage?.Invoke("사용 불가능한 칭호입니다...");
            return false;
        }

        //로그인된 상황임!!!

        string oldTitle = UserData.Local.userTitle;
        if (oldTitle == newTitle)
        {
            displayMessage?.Invoke("같은 칭호로는 변경할 수 없어요!");
            return false;
        }

        //본격적으로 변경 시작
        displayMessage?.Invoke("칭호 변경 작업 진행 중...");

        //0. Firebase에다가 로컬 유저데이터 올리기


        try
        {
            await CurrentUserDataRef.Child("userTitle").SetValueAsync(newTitle);
            displayMessage?.Invoke("DB에 새 칭호 적용 완료...");
        }
        catch (FirebaseException fe)
        {
            displayMessage?.Invoke("DB 업데이트 실패! 칭호 등록을 취소합니다...");
            Debug.LogError($"파이어베이스 에러: {fe.Message}");
            return false;
        }

        //TODO: 2. 포톤 커스텀프로퍼티화해서 저장해주기.


        //여기까지 왔으면 모든 닉네임 목록에도 새 닉네임을 넣었고, 기존 닉네임은 지웠고,
        //DB의 이 유저의 닉네임을 설정해줬고,
        //인증 정보의 디스플레이네임도 설정해준 것임.

        //남은 처리: 로컬 유저데이터의 이름 변경해주기.
        UserData.Local.ChangeUserTitle(newTitle);
        ExitGames.Client.Photon.Hashtable cp = new();
        //TODO: 여기부터!!!
        cp.Add("userTitle", newTitle);
        PhotonNetwork.LocalPlayer.SetCustomProperties(cp);
        //최종적으로 true 반환해주기!!
        return true;
    }

    public async Task<bool> CheckIfNameReservedAndReset(string newName, Action<string> displayMessage = null)
    {
        //가장 먼저 엣지케이스 처리: 쓸 수 없는 닉네임은 쳐내기
        //TODO: 나중에 정규식 넣어서 필터링할수도 있겠음
        if (newName == "DEFAULT" || newName.IsNullOrEmpty())
        {
            displayMessage?.Invoke("사용 불가능한 닉네임입니다...");
            
            return false; 

        }

        //로그인된 상황임!!!

        //HACK: 강욱 - 1006: 닉네임 변경 기능으로도 활용할 것이기 때문에 예외처리 추가(바꿀 이름과 원래 이름이 같은 경우)
        string oldName = Auth.CurrentUser.DisplayName;
        if (oldName == newName)
        {
            displayMessage?.Invoke("같은 이름으로는 변경할 수 없어요!");
            return false;
        }

        displayMessage?.Invoke($"새 닉네임 {newName} 중복 검사 중...");
        //DB의 '이미 존재하는 닉네임' 노드에 새 닉네임이 있는지 검사하는 트랜잭션 수행.
        var result = await AllNamesRef.Child(newName).RunTransaction(mutableData =>
        {
            if (mutableData.Value != null)
            {
                //이미 mutableData가 존재: 중복 닉으로 존재한다는 뜻.
                displayMessage?.Invoke($"닉네임 중복 확인!");
                return TransactionResult.Abort();
            }
            
            //'닉네임'이라는 키의 '값'을 현재 유저의 uid로
            mutableData.Value = Auth.CurrentUser.UserId;

            //


            //커밋 요청 부분
            return TransactionResult.Success(mutableData);
        });

        //result.Exists == false면, 중복 닉이란 뜻임.
        if ((string)result.Value != Auth.CurrentUser.UserId)
        {
            displayMessage?.Invoke("이미 사용중인 닉네임입니다.");
            return false;
        }

        //여기까지 내려왔으면 본격적으로 변경 시작
        displayMessage?.Invoke("닉네임 변경 작업 진행 중...");

        //0. 전체 닉네임 풀에서 원래 이름 삭제 시도

        if (!oldName.IsNullOrEmpty()) //이 분기 필요해보임....
        {
            try
            {
                await AllNamesRef.Child(oldName).RemoveValueAsync();
                displayMessage?.Invoke("기존 닉네임 삭제 처리...");
            }
            catch (FirebaseException fe)
            {
                displayMessage?.Invoke("DB 업데이트 실패! 닉네임 등록을 취소합니다...");
                //실패했으니 닉네임 풀 롤백
                await AllNamesRef.Child(newName).RemoveValueAsync();
                await AllNamesRef.Child(oldName).SetValueAsync(Auth.CurrentUser.UserId);
                Debug.LogError($"파이어베이스 에러: {fe.Message}");
                return false;
            }
        }

        //1. DB의 이 유저의 노드에 있는 닉네임 변경 시도
        try
        {
            await CurrentUserDataRef.Child("userName").SetValueAsync(newName);
            displayMessage?.Invoke("DB에 새 닉네임 적용 완료...");
        }
        catch (FirebaseException fe)
        {
            displayMessage?.Invoke("DB 업데이트 실패! 닉네임 등록을 취소합니다...");
            //실패했으니 닉네임 풀 롤백
            await AllNamesRef.Child(newName).RemoveValueAsync();
            await AllNamesRef.Child(oldName).SetValueAsync(Auth.CurrentUser.UserId);
            Debug.LogError($"파이어베이스 에러: {fe.Message}");
            return false;
        }

        //2. 인증 정보의 DisplayName 변경 시도

        try { 
        await Auth.CurrentUser.UpdateUserProfileAsync(new UserProfile { DisplayName = newName });
            displayMessage?.Invoke("인증 정보에 새 닉네임 적용 완료...");

        }
        catch (FirebaseException fe)
        {
            displayMessage?.Invoke("인증 정보 업데이트 실패! 닉네임 등록을 취소합니다...");
            //실패했으니 DB 롤백
            await CurrentUserDataRef.Child("userName").SetValueAsync(oldName);
            //실패했으니 닉네임 풀 롤백
            await AllNamesRef.Child(newName).RemoveValueAsync();
            await AllNamesRef.Child(oldName).SetValueAsync(Auth.CurrentUser.UserId);
            Debug.LogError($"파이어베이스 에러: {fe.Message}");
            return false;
        }

        //여기까지 왔으면 모든 닉네임 목록에도 새 닉네임을 넣었고, 기존 닉네임은 지웠고,
        //DB의 이 유저의 닉네임을 설정해줬고,
        //인증 정보의 디스플레이네임도 설정해준 것임.

        //남은 처리: 로컬 유저데이터의 이름 변경해주기.
        UserData.Local.ChangeUserName(newName);
        //TODO: 포톤에까지 접속 후에 닉네임 변경할 때의 처리 필요.
        if (PhotonNetwork.IsConnected) PhotonNetwork.LocalPlayer.NickName = newName;

        //최종적으로 true 반환해주기!!
        return true;
    }

    public async void GuestSignIn(UnityAction<FirebaseUser> callback = null)
    {
        try
        {
            
            AuthResult result = await Auth.SignInAnonymouslyAsync();

            UserData guestUser = new(true);
            string json = JsonUtility.ToJson(guestUser);
            await CurrentUserDataRef.SetRawJsonValueAsync(json);
            UserData.Local = guestUser;
            await Auth.CurrentUser.UpdateUserProfileAsync(new UserProfile() { DisplayName = UserData.Local.userName });
            callback?.Invoke(result.User);
        }
            catch (FirebaseException fe)
        {
            Debug.LogError($"파이어베이스 에러: {fe.Message}");
            callback?.Invoke(null);
        }

    }

    public async Task RefetchUserData()
    {
        try
        {
            DataSnapshot snap = await CurrentUserDataRef.GetValueAsync();
            if (snap.Exists)
            {
                string json = snap.GetRawJsonValue();
                UserData.Local = JsonUtility.FromJson<UserData>(json);
                Debug.Log("유저데이터 다시 받아옴.");
            }
        }
        catch (FirebaseException fe)
        {
            Debug.Log(fe.Message);
        }
    }
    public async void SignIn(string email, string pw, Action<FirebaseUser> callback = null)
    {
        try
        {
            //이메일과 비밀번호로 로그인 시도
            //TODO: 간혹가다 로컬 유저데이터가 똑바로 적용되지 않는 문제가 있는데 이거 해결해야 함.
            AuthResult result = await Auth.SignInWithEmailAndPasswordAsync(email, pw);
            DataSnapshot snap = await CurrentUserDataRef.GetValueAsync();
            if (snap.Exists)
            {
                //DB로부터 유저데이터를 가져오는 데 성공함
                Debug.Log("DB에서 유저데이터 가져옴.");
                //HACK: 강욱 - 1007: 이 안에 있던 스냅샷 제이슨으로 바꾸고 FromJson해서 UserData.Local 설정하는 부분 괄호 바깥으로 빼봄.
                //자꾸 이 부분에서 왜 버그가 있는지 모르겠음(엉뚱한 유저데이터를 들고 게임에 들어가버림..)
            }
            else
            {
                //DB로부터 유저데이터를 가져오는 데 실패함
                throw new Exception("유저데이터를 가져오지 못했습니다");
            }
            string json = snap.GetRawJsonValue();

            UserData data = JsonUtility.FromJson<UserData>(json);
            UserData.Local = data;

            Debug.Log($"가져온 유저데이터 json: {json}");

            //OnLogin();

            Debug.Log($"로그인 성공함.uid:{result.User.UserId}");

            Debug.Log("로그인 성공");
            callback?.Invoke(result.User);
        }
        catch (FirebaseException fe)
        {
            Debug.LogError($"파이어베이스 에러: {fe.Message}");
            callback?.Invoke(null);
        }

        try
        {
            await FetchCustomizationDataFromFirebase();
            //아마 기존 유저데이터로는 이 부분에서 에러가 날 것... (DB상에서 경로가 아예 없음)
        } catch (FirebaseException fe)
        {
            Debug.LogError($"파이어베이스 에러: {fe.Message}");
            callback?.Invoke(null);

        }
    }

    //public async void UserInfoChange(string displayName, string photoURL, Action<FirebaseUser> callback)
    //{
    //    try
    //    {
    //        UserProfile profile = new UserProfile();
    //        //저장 가능한 유저의 프로필은 표시이름(DisplayName)과 사진, 추가적으로 email과 전화번호 정도임
    //        //그럼 다른 필요한 정보들은 FirebaseDB에 저장하면 됨.
    //        profile.DisplayName = displayName;
    //        profile.PhotoUrl = new Uri(photoURL);
    //        await Auth.CurrentUser.UpdateUserProfileAsync(profile);
    //        callback?.Invoke(Auth.CurrentUser);
    //    }
    //    catch (FirebaseException fe)
    //    {
    //        Debug.LogError($"파이어베이스 에러: {fe.Message}");
    //        callback?.Invoke(null);
    //    }
    //}

    public async void SignOut()
    {
        try
        {
            if (!UserData.Local.isGuest)
            {
                Auth.SignOut();
                //게스트 유저가 아니면 그냥 로그아웃만 하면 됨.
            }
            else
            {
                //게스트 유저인 경우

                //모든 닉네임 목록에서 이 유저의 닉네임을 제거
                await AllNamesRef.Child(Auth.CurrentUser.DisplayName).RemoveValueAsync();

                //이 유저데이터 참조의 부모(Users/uid) 제거
                await CurrentUserDataRef.Parent.RemoveValueAsync();

                //이 유저 인증 삭제
                await Auth.CurrentUser.DeleteAsync();
            }
        }
        catch (FirebaseException fe)
        {
            Debug.LogError($"파이어베이스 에러: {fe.Message}");
        }
    }
    public void OnApplicationQuit()
    {
        //정상 종료 시에 그냥 await하지 않고 게스트유저의 정보 삭제해버림. 날아가면 날아가는 거고 아니어도 그만임.
        if (UserData.Local.isGuest)
        {
        AllNamesRef.Child(Auth.CurrentUser.DisplayName).RemoveValueAsync();
        CurrentUserDataRef.Parent.RemoveValueAsync();
        Auth.CurrentUser.DeleteAsync();
        }
    }

    private async Task FetchCustomizationDataFromFirebase()
    {
        try
        {
            // 1. DB의 커스터마이징 정보에서 스냅샷 수신 시도
            var customizationSnapshot = await CurrentUserCustomizationDataRef.GetValueAsync();
            if (customizationSnapshot.Exists)
            {
                // 2. 잘 받았다면, json화 후에 CustomizationData 객체로 변환하고, 로컬에 덮어씌워줌.
                // 로컬의 정보는 포톤 로그인 때에 커스텀프로퍼티화되어 넘어갈 예정임.
                string json = customizationSnapshot.GetRawJsonValue();
                CustomizationData localCustomization = JsonUtility.FromJson<CustomizationData>(json);
                CustomizationData.Local = localCustomization;
            } else
            {
                Debug.Log("아마 옛날에 만든 유저라 DB경로상 {uid}/CustomizationData가 없는 것같음.");
            }
        }
        catch (FirebaseException fe)
        {
            Debug.Log($"파이어베이스 오류남: 1. 이 유저의 노드 구조가 똑바로가 아니거나\n2. 경로에 오타가 있거나\n3. 네트워크가 이상하거나\n 암튼 확인 필요\n{fe.Message}");
        }
    }
}
