# 🍎 과일가게 키오스크 – Unity C# 스크립트 구조

## 씬 구성 (Build Settings 등록 필요)

| 씬 이름        | 설명                                  |
|--------------|-------------------------------------|
| IntroScene   | 시작 화면, 픽업/배송 선택               |
| MenuScene    | 메뉴 선택 (일반/컵/선물세트)            |
| CartScene    | 장바구니 확인 및 수정                   |
| PaymentScene | 결제 방법 선택 (입금/카드/포인트)        |
| PrintScene   | 주문서 / 영수증 출력                    |

---

## 스크립트 목록

### 📁 Intro
| 파일 | 역할 |
|-----|------|
| `IntroManager.cs` | 시작 버튼, 픽업/배송 선택 → MenuScene 이동 |
| `OrderSession.cs` | DontDestroyOnLoad 싱글톤 – 주문 데이터 관리 |
| `CartItem.cs` | 장바구니 아이템 데이터 모델 |

### 📁 Menu
| 파일 | 역할 |
|-----|------|
| `MenuManager.cs` | 탭 전환, 장바구니 뱃지 표시, CartScene 이동 |
| `NormalFruitPanel.cs` | 일반과일 – 등급(당도/크기) + 커팅 옵션 |
| `CupFruitPanel.cs` | 컵과일 – 구성(단품/혼합) + 크기 옵션 |
| `GiftFruitPanel.cs` | 선물세트 – 크기 + 포장 옵션 |

### 📁 Cart
| 파일 | 역할 |
|-----|------|
| `CartManager.cs` | 장바구니 목록 렌더링, 합계, 배송 무게 검증 |
| `CartItemRow.cs` | 장바구니 행 프리팹 스크립트 (삭제 포함) |

### 📁 Payment
| 파일 | 역할 |
|-----|------|
| `PaymentManager.cs` | 입금/카드/포인트 선택, 포인트 차감, PrintScene 이동 |

### 📁 Print
| 파일 | 역할 |
|-----|------|
| `PrintManager.cs` | 주문서/영수증 생성, 출력, 새 주문 초기화 |

---

## 흐름도

```
IntroScene
  └─ [시작하기] → 픽업/배송 선택
       └─ MenuScene
            ├─ 일반과일 (등급 / 크기 / 커팅)
            ├─ 컵과일   (구성 / 크기)
            └─ 선물세트 (크기 / 포장)
                 └─ CartScene
                      └─ 배송 무게 검증 (3kg↑)
                           └─ PaymentScene
                                ├─ 입금
                                ├─ 카드
                                └─ 포인트
                                     └─ PrintScene
                                          ├─ 주문서 출력
                                          └─ 영수증 출력
                                               └─ [새 주문] → IntroScene
```

---

## Unity 설정 메모

- `OrderSession` 오브젝트는 **IntroScene에서만 생성** (DontDestroyOnLoad)
- 각 Panel 스크립트는 해당 패널 GameObject에 컴포넌트로 부착
- `CartItemRow` 프리팹에 Text 4개 + Button 1개 구성 필요
- `Dropdown`, `InputField`, `Toggle`은 **Legacy UI** 또는 **TMP** 버전 선택 통일
