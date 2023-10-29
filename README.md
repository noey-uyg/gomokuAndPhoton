# gomokuAndPhoton
- 포톤과 유니티를 활용한 블라인드 오목입니다.
- 블라인드 오목이므로, 흑돌 백돌 모두 승리규칙 외의 규칙을 정해두지 않습니다.
- 코드는 Scripts 폴더에 있습니다.

# Day 1(2023 - 10 - 26)
- 싱글톤 패턴, 오브젝트 풀링 적용
- 오목은 Tilemap으로 구현
- Tilemap collider 2D Component를 추가해서 마우스와 cell 좌표를 통해 오목알을 착수할 수 있도록 구현
- 규칙없는 오목

# Day 2(2023 - 10 - 28)
- 포톤 연동 
- 간단한 UI 배치 
- 닉네임 설정, 로비 입장, 방 입장, 공지, 채팅 구현 

# Day 3(2023 - 10 -29)
- 게임시작(솔로 안됨) 
- 오목알 바꾸기 구현 
- 상대방이 놓는 오목알도 내 오목알 색깔과 같게 배치(블라인드 오목)
- 오목의 승패가 결정된 후 상대방이 놓은 오목알과 내가 놓은 오목알을 보여줌 
- 클라이언트 간 동기화 완료 
