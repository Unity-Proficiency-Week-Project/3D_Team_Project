# 프로젝트 이름 

TerraFort (Terra(땅) + Fortress(요새))

## 📖 목차
1. [프로젝트 소개](#프로젝트-소개)
2. [주요기능](#주요기능)
3. [개발기간](#개발기간)
4. [Trouble Shooting](#trouble-shooting)
   
## 프로젝트 소개
 Unity 숙련 팀 프로젝트
 

## 주요기능

- 기능 1 : 인풋 액션을 이용하여, 플레이어의 동작 및 기능 설정

![Image](https://github.com/user-attachments/assets/01b72c4f-c463-4650-a9b1-6afd7a627d4a)

- 기능 2 : 날씨 시스템 - 지면의 온도에 따라 해당 지면의 날씨 랜덤으로 설정 (25'C 이상시 눈 확률 감소, 35'C 이상시 강우 확률 증가)
![Image](https://github.com/user-attachments/assets/599bc83f-58b8-40e0-8fe6-ba1129ff1a15)

- 기능 3 : 건축 시스템 
![Image](https://github.com/user-attachments/assets/40cfc62c-5adb-4164-86ee-4f538ec8e73a)

- 기능 4 : 제작 시스템
![Image](https://github.com/user-attachments/assets/813d90cd-f379-4c80-a074-76fdac323f48)

- 기능 5 : 환경설정
![Image](https://github.com/user-attachments/assets/d6b48fb2-8148-4602-8bbb-109f6a9f78f4)

- 기능 6 : 스테미너를 사용해서 활동을 하고 배고픔이 다 소진 될 경우 체력이 감소됨 그리고 체력이 0이 된다면 플레이어가 사망했습니다. 팝업이 나옴

- 기능 7 : 소모 아이템 섭취 시, 배고픔과 체력 회복 가능

- 기능 8 : 몹의 종류가 근거리 / 원거리 / 공격하지 않는 몹으로, 몹의 설정값에 따라 공격의 범위를 설정 +) 공격을 하지 않는 몹의 경우 플레이어로부터 도망을 감
  
- 기능 8 : Enemy 공격, 적 처치 시에 랜덤으로 자원이 드랍됨

  ![Image](https://github.com/user-attachments/assets/cfbd9e67-5326-43a2-9bcc-4edd7a879b3e)

- 기능 9 : 인벤토리에서 아이템 사용,장착 및 드랍을 할 수 있음
![Image](https://github.com/user-attachments/assets/fe91852c-90ba-47eb-b7b2-4e75d284e869)

- 기능 10 : 플레이어가 자원을 캘 경우, 자원이 드랍이 되고, 상호 작용 키를 눌러 인벤토리에 추가 할 수 있음
  ![Image](https://github.com/user-attachments/assets/fe776a12-feb6-4a52-9dd4-6eb135d4be19)

- 기능 11 : 자원 채집
  ![Image](https://github.com/user-attachments/assets/c691d2e2-388a-45ae-a2a4-9c9099c88af1)

- 기능 12 : 퀘스트
 ![Image](https://github.com/user-attachments/assets/558c5e6e-ab3b-4a29-a704-6200106a3ea9)


## ⏲️ 개발기간
- 2024.03.12(화) ~ 2024.03.19(화)

## Trouble Shooting

### Button

델리게이트를 사용하여 아이템의 타입에 따라 버튼의 기능과 텍스트가 변하도록 코드를 구현 -> 아이템이 달라져도 버튼이 "착용"으로 고정이 됨 -> 기존 if문을 통한 조건을 switch문으로 변경 -> 가독성 및 오류 해결
```
switch (selectedItem.itemData.itemType)
        {
            case ItemType.Consumable:
                actionButtonText.text = "사용";
                currentAction = () => OnUseButton();
                break;

            case ItemType.Equipable:
                if (!slots[index].equipped)
                {
                    actionButtonText.text = "장착";
                    currentAction = () => OnEquipButton();
                }
                else
                {
                    actionButtonText.text = "해제";
                    currentAction = () => UnEquip(index);
                }
                actionButton.SetActive(true);
                break;

            default:
                actionButtonText.text = string.Empty;
                currentAction = null;
                actionButton.SetActive(false);
                break;
        }
```

### ViewPort

건축 UI 스크롤 뷰 생성 중 3D 오브젝트가 스크롤 뷰의 뷰포트 바깥에서도 표시됨 -> UI 카메라 범위 조절, 레이어 마스크 변경 등 시도하였으나, 실패 -> 뷰포트의 범위를 가져와 뷰포트 바깥에 존재하는 슬롯을 비활성화 -> 해결
```
public void SetActiveSlotObject()
    {
        Vector3[] viewportCorners = new Vector3[4];

        // 뷰포트의 각 코너의 위치 가져오기
        viewport.GetWorldCorners(viewportCorners);

        float padding = viewport.rect.height * 0.1f;

        // 뷰포트좌표의 최대값(우측 상단), 최소값(촤측 하단) 가져오기
        Vector2 viewportMin = new Vector2(viewportCorners[0].x, viewportCorners[0].y); 
        Vector2 viewportMax = new Vector2(viewportCorners[2].x, viewportCorners[2].y - viewportPadding); 

        foreach (var obj in slotObejctList)
        {
            // 슬롯 오브젝트 위치 가져오기
            Vector3 worldPosition = obj.transform.position;

            // 현재 슬롯이 뷰포트 범위 안에 위치하는지 검사 후 활성화/비활성화
            if (worldPosition.x >= viewportMin.x && worldPosition.x <= viewportMax.x &&
                worldPosition.y >= viewportMin.y && worldPosition.y <= viewportMax.y)
            {
                obj.SetActive(true); 
            }
            else
            {
                obj.SetActive(false); 
            }
        }
```

### Random Respawn

일정 시간이 지나면 랜덤한 위치에서 적 및 자원이 리스폰 해야하는데, 땅 중심에서만 리스폰 -> Vector3 randomOffset의 위치를 구함 -> Offset의 값을 randomPostion에 더함 -> 해결

```
private Vector3 GetRandomCubeSpawnPosition()
    {
        if (spawnPoints.Count == 0)
        {
            Debug.LogError("스폰 가능한 큐브가 없습니다!");
            return Vector3.zero;
        }
        Transform randomCube;
        for (int i = 0; i < 10; i++) // 최대 10번 시도
        {
            randomCube = spawnPoints[Random.Range(0, spawnPoints.Count)];
            Vector3 randomOffset = new Vector3(Random.Range(-randomCube.localScale.x / 2, randomCube.localScale.x / 2), // X축 랜덤 오프셋
                                                randomCube.position.y + 1f,  // Y축은 바닥에 박히지 않도록 기본 값 유지
                                                Random.Range(-randomCube.localScale.z / 2, randomCube.localScale.z / 2)  // Z축 랜덤 오프셋
                                               );
            Vector3 randomPosition = randomCube.position + randomOffset; // 최종 랜덤 위치

            if (!IsPositionOccupied(randomPosition))
            {
                return randomPosition;
            }
        }
        return Vector3.zero;
    }
```

### Animation

BlendTree로 애니메이션 구현 -> 애니메이션 작동이 자연스럽지 못함 (Idle Animation이 Wandering 상태로, 너무 느리게 작동하고, 공격 애니메이션이 이상함 

느리게 작동 수정 : Threshold 값(파라미터) 우측 애니메이션 속도에 MoveSpeed 파라미터 값을 할당, Idle 상태 : 0.01배속, Walk 상태 : 0.5배속, Run 상태 : 1배속 인것을 확인 -> 해당 값을 수정

공격 애니메이션 : 직접 실행하며, 상태에 맞지 않는 애니메이션을 체크하여, EnemyAI.cs에서 agent.isStopped를 조정함, 그리고 애니메이션을 수작업으로 값을 조정하며, 제작하였음.
