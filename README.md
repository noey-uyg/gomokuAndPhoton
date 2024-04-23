# gomokuAndPhoton
- 포톤과 유니티를 활용한 블라인드 오목입니다.
- 블라인드 오목이므로, 흑돌 백돌 모두 승리규칙 외의 규칙을 정해두지 않습니다.
- 코드는 Scripts 폴더에 있습니다.

# --
- 싱글톤 패턴, 오브젝트 풀링 적용
- 오목판은 Tilemap으로 구현
- Tilemap collider 2D Component를 추가해서 마우스와 cell 좌표를 통해 오목알을 착수할 수 있도록 구현
- 규칙없는 오목
- 포톤 연동 
- 닉네임 설정, 로비 입장, 방 입장, 공지, 채팅 구현 
- 게임시작(솔로 안됨) 
- 오목알 바꾸기 구현 
- 상대방이 놓는 오목알도 내 오목알 색깔과 같게 배치(블라인드 오목)
- 오목의 승패가 결정된 후 상대방이 놓은 오목알과 내가 놓은 오목알을 보여줌 
- 클라이언트 간 동기화

# 로그인
<img width="80%" src ="https://github.com/noey-uyg/gomokuAndPhoton/assets/105009308/eeaba493-74b1-4f94-a2c8-d9e2f3639ba5"/>

- 닉네임을 입력하고 연결하기 클릭 시 포톤 네트워크에 연결
- PhotonNetwork.ConnectUsingSettings()으로 서버와 사용자를 연결
- InputText에 작성된 Text를 사용자 닉네임으로 저장

# 방 만들기
<img width="80%" src ="https://github.com/noey-uyg/gomokuAndPhoton/assets/105009308/57a8a04f-7c6b-46b0-895f-3a4cc3256dd3"/>

- 방 이름을 작성하고 방 만들기를 누르면 방 생성
- RoomOptions를 이용해 방 인원 2명으로 제한 등 방 제한 옵션 생성
- RoomOptions의 GameState를 통해 게임중인지 대기중인지 표시

# 방 입장, 경고창, 채팅
<img width="80%" src ="https://github.com/noey-uyg/gomokuAndPhoton/assets/105009308/37bfbd96-224d-499d-bd5c-dc6115593933"/>

- 방에 입장하면 OnPlaerEnteredRoom함수를 재정의하여 누가 참가했는지 알림
- RPC를 통해 알림창 동기화
- ScrollView를 통해 채팅창 구현
- 채팅을 보낼 때마다 RPC를 호출을 통해 동기화

# 방 조작 권한
<img width="80%" src ="https://github.com/noey-uyg/gomokuAndPhoton/assets/105009308/125a8626-aabe-4238-87b9-68a816360dc0"/>

- (IsMasterClient() && PhotonNetwork.CurrentRoom.PlayerCount != 1) 제약으로 방장이 아닐 때 또는 1명일 때 게임 시작을 할 수 없도록 만듬
- 돌 바꾸기 또한 같은 제약으로 방장만 돌을 바꿀 수 있음

# 게임 시작, 게임 오버
<img width="80%" src ="https://github.com/noey-uyg/gomokuAndPhoton/assets/105009308/c573ffc3-ec0a-498f-b1a8-94515aff338e"/>

- 보드판을 타일맵으로 만들고, 마우스 위치를 월드 좌표로 변환하여 타일맵에 마우스가 올라가있는지 판단
- 착수 시 해당 셀의 이미지 활성화 및 본인의 컬러로 설정 후 턴 변경 [RPC 동기화]
- 블라인드 오목으로 상대방이 놓은 돌도 본인의 돌로 표시
- 매 클릭 시 승리 조건 검사 함수 동작 5개의 연속된 돌이 있는 경우 승리
- 누군가 승리할 경우 상대방과 자신이 놓은 돌을 표시
