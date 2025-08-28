# 2DProject

간단한 2D RPG 실험/학습용 프로젝트입니다. **APIServer(.NET Core)**, **GameClient(Unity)**, **Docker 구성**, **부하 테스트(nGrinder)** 등 서브 프로젝트를 포함하고 있습니다.

## 폴더 안내

| 폴더 | 설명 |
|---|---|
| **APIServer/** | ASP.NET Core 기반 API 서버. 인증/인벤토리/퀘스트 등 백엔드 기능을 제공합니다. |
| **GameClient/** | Unity 기반 2D 클라이언트. 씬/캐릭터/전투/UI 등 게임 플레이 로직을 포함합니다. |
| **Docker/** | 개발/테스트 환경을 위한 Docker 및 Compose 설정(예: Redis, MySQL 등). |
| **nGrinder/** | API 부하/성능 검증용 스크립트와 시나리오 예제. |

---
