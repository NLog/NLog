;Language: Korean (1042)
;By linak linak@korea.com ( ~ V2.0 BETA3 ) By kippler@gmail.com(www.kipple.pe.kr) ( V2.0 BETA3 ~ ) (last update:2007/09/05)

!insertmacro LANGFILE "Korean" "Korean"

!ifdef MUI_WELCOMEPAGE
  ${LangFileString} MUI_TEXT_WELCOME_INFO_TITLE "$(^NameDA) 설치를 시작합니다."
  ${LangFileString} MUI_TEXT_WELCOME_INFO_TEXT "이 프로그램은 당신의 컴퓨터에 $(^NameDA)(을)를 설치할 것입니다.$\r$\n$\r$\n설치를 시작하기 전 가능한 한 모든 프로그램을 종료하여 주시기 바랍니다. 이는 재부팅을 하지 않고서도 시스템 파일을 수정할 수 있게 해줍니다.$\r$\n$\r$\n$_CLICK"
!endif

!ifdef MUI_UNWELCOMEPAGE
  ${LangFileString} MUI_UNTEXT_WELCOME_INFO_TITLE "$(^NameDA) 제거를 시작합니다."
  ${LangFileString} MUI_UNTEXT_WELCOME_INFO_TEXT "이 프로그램은 당신의 컴퓨터에서 $(^NameDA)(을)를 제거할 것입니다.$\r$\n$\r$\n제거를 시작하기 전에 $(^NameDA)(을)를 종료하여 주시기 바랍니다.$\r$\n$\r$\n$_CLICK"
!endif

!ifdef MUI_LICENSEPAGE
  ${LangFileString} MUI_TEXT_LICENSE_TITLE "사용권 계약"
  ${LangFileString} MUI_TEXT_LICENSE_SUBTITLE "$(^NameDA)(을)를 설치하시기 전에 사용권 계약 내용을 살펴보시기 바랍니다."
  ${LangFileString} MUI_INNERTEXT_LICENSE_BOTTOM "내용에 동의하셨다면 '동의함'을 눌러 주세요. $(^NameDA)(을)를 설치하기 위해서는 반드시 내용에 동의하셔야 합니다."
  ${LangFileString} MUI_INNERTEXT_LICENSE_BOTTOM_CHECKBOX "내용에 동의하셨다면 아래 사항을 선택해 주세요. $(^NameDA)(을)를 설치하기 위해서는 반드시 내용에 동의하셔야 합니다. $_CLICK"
  ${LangFileString} MUI_INNERTEXT_LICENSE_BOTTOM_RADIOBUTTONS "내용에 동의하셨다면 첫 번째 사항을 선택해 주세요. $(^NameDA)(을)를 설치하기 위해서는 반드시 내용에 동의하셔야 합니다. $_CLICK"
!endif

!ifdef MUI_UNLICENSEPAGE
  ${LangFileString} MUI_UNTEXT_LICENSE_TITLE "사용권 계약 동의"
  ${LangFileString} MUI_UNTEXT_LICENSE_SUBTITLE "$(^NameDA)(을)를 제거하시기 전에 사용권 계약 내용을 살펴보시기 바랍니다."
  ${LangFileString} MUI_UNINNERTEXT_LICENSE_BOTTOM "내용에 동의하셨다면 '동의함'을 눌러 주세요. $(^NameDA)(을)를 제거하기 위해서는 반드시 내용에 동의하셔야 합니다."
  ${LangFileString} MUI_UNINNERTEXT_LICENSE_BOTTOM_CHECKBOX "내용에 동의하셨다면 아래 사항을 선택해 주세요. $(^NameDA)(을)를 제거하기 위해서는 반드시 내용에 동의하셔야 합니다. $_CLICK"
  ${LangFileString} MUI_UNINNERTEXT_LICENSE_BOTTOM_RADIOBUTTONS "내용에 동의하셨다면 첫 번째 사항을 선택해 주세요. $(^NameDA)(을)를 제거하기 위해서는 반드시 내용에 동의하셔야 합니다. $_CLICK"
!endif

!ifdef MUI_LICENSEPAGE | MUI_UNLICENSEPAGE
  ${LangFileString} MUI_INNERTEXT_LICENSE_TOP "사용권 계약 동의 사항의 나머지 부분을 보시려면 [Page Down] 키를 눌러 주세요."
!endif

!ifdef MUI_COMPONENTSPAGE
  ${LangFileString} MUI_TEXT_COMPONENTS_TITLE "구성 요소 선택"
  ${LangFileString} MUI_TEXT_COMPONENTS_SUBTITLE "설치하고자 하는 $(^NameDA)의 구성 요소를 선택해 주세요."
!endif

!ifdef MUI_UNCOMPONENTSPAGE
  ${LangFileString} MUI_UNTEXT_COMPONENTS_TITLE "구성 요소 선택"
  ${LangFileString} MUI_UNTEXT_COMPONENTS_SUBTITLE "제거하고자 하는 $(^NameDA)의 구성 요소를 선택해 주세요."
!endif

!ifdef MUI_COMPONENTSPAGE | MUI_UNCOMPONENTSPAGE
  ${LangFileString} MUI_INNERTEXT_COMPONENTS_DESCRIPTION_TITLE "상세 설명"
  !ifndef NSIS_CONFIG_COMPONENTPAGE_ALTERNATIVE
    ${LangFileString} MUI_INNERTEXT_COMPONENTS_DESCRIPTION_INFO "상세 설명을 보고 싶으신 부분에 마우스를 올려놓으세요."
  !else
    ${LangFileString} MUI_INNERTEXT_COMPONENTS_DESCRIPTION_INFO "상세 설명을 보고 싶으신 부분에 마우스를 올려놓으세요."
  !endif
!endif

!ifdef MUI_DIRECTORYPAGE
  ${LangFileString} MUI_TEXT_DIRECTORY_TITLE "설치 위치 선택"
  ${LangFileString} MUI_TEXT_DIRECTORY_SUBTITLE "$(^NameDA)(을)를 설치할 폴더를 선택해 주세요."
!endif

!ifdef MUI_UNDIRECTORYPAGE
  ${LangFileString} MUI_UNTEXT_DIRECTORY_TITLE "제거 위치 선택"
  ${LangFileString} MUI_UNTEXT_DIRECTORY_SUBTITLE "$(^NameDA)(을)를 제거할 폴더를 선택해 주세요."
!endif

!ifdef MUI_INSTFILESPAGE
  ${LangFileString} MUI_TEXT_INSTALLING_TITLE "설치중"
  ${LangFileString} MUI_TEXT_INSTALLING_SUBTITLE "$(^NameDA)(을)를 설치하는 동안 잠시 기다려 주세요."
  ${LangFileString} MUI_TEXT_FINISH_TITLE "설치 완료"
  ${LangFileString} MUI_TEXT_FINISH_SUBTITLE "설치가 성공적으로 완료되었습니다."
  ${LangFileString} MUI_TEXT_ABORT_TITLE "설치 취소"
  ${LangFileString} MUI_TEXT_ABORT_SUBTITLE "프로그램 설치가 취소되었습니다."
!endif

!ifdef MUI_UNINSTFILESPAGE
  ${LangFileString} MUI_UNTEXT_UNINSTALLING_TITLE "제거중"
  ${LangFileString} MUI_UNTEXT_UNINSTALLING_SUBTITLE "$(^NameDA)(을)를 제거하는 동안 잠시 기다려 주시기 바랍니다."
  ${LangFileString} MUI_UNTEXT_FINISH_TITLE "제거 마침"
  ${LangFileString} MUI_UNTEXT_FINISH_SUBTITLE "프로그램을 성공적으로 제거하였습니다."
  ${LangFileString} MUI_UNTEXT_ABORT_TITLE "프로그램 제거 취소"
  ${LangFileString} MUI_UNTEXT_ABORT_SUBTITLE "프로그램 제거가 취소되었습니다."
!endif

!ifdef MUI_FINISHPAGE
  ${LangFileString} MUI_TEXT_FINISH_INFO_TITLE "$(^NameDA) 설치 완료"
  ${LangFileString} MUI_TEXT_FINISH_INFO_TEXT "$(^NameDA)의 설치가 완료되었습니다. 설치 프로그램을 마치려면 '마침' 버튼을 눌러 주세요."
  ${LangFileString} MUI_TEXT_FINISH_INFO_REBOOT "$(^NameDA)의 설치를 완료하기 위해서는 시스템을 다시 시작해야 합니다. 지금 재부팅 하시겠습니까?"
!endif

!ifdef MUI_UNFINISHPAGE
  ${LangFileString} MUI_UNTEXT_FINISH_INFO_TITLE "제거 완료"
  ${LangFileString} MUI_UNTEXT_FINISH_INFO_TEXT "$(^NameDA)의 제거가 완료 되었습니다."
  ${LangFileString} MUI_UNTEXT_FINISH_INFO_REBOOT "$(^NameDA)의 제거를 완료하기 위해서는 시스템을 다시 시작해야 합니다. 지금 재부팅 하시겠습니까?"
!endif

!ifdef MUI_FINISHPAGE | MUI_UNFINISHPAGE
  ${LangFileString} MUI_TEXT_FINISH_REBOOTNOW "지금 재부팅 하겠습니다."
  ${LangFileString} MUI_TEXT_FINISH_REBOOTLATER "나중에 재부팅 하겠습니다."
  ${LangFileString} MUI_TEXT_FINISH_RUN "$(^NameDA) 실행하기(&R)"
  ${LangFileString} MUI_TEXT_FINISH_SHOWREADME "Readme 파일 보기(&S)"
  ${LangFileString} MUI_BUTTONTEXT_FINISH "마침"  
!endif

!ifdef MUI_STARTMENUPAGE
  ${LangFileString} MUI_TEXT_STARTMENU_TITLE "시작 메뉴 폴더 선택"
  ${LangFileString} MUI_TEXT_STARTMENU_SUBTITLE "프로그램의 바로 가기 아이콘이 생성될 시작 메뉴 폴더 선택."
  ${LangFileString} MUI_INNERTEXT_STARTMENU_TOP "프로그램의 바로 가기 아이콘이 생성될 시작 메뉴 폴더를 선택하세요. 새로운 폴더를 생성하려면 폴더 이름을 입력하세요."
  ${LangFileString} MUI_INNERTEXT_STARTMENU_CHECKBOX "바로 가기 아이콘을 만들지 않겠습니다."
!endif

!ifdef MUI_UNCONFIRMPAGE
  ${LangFileString} MUI_UNTEXT_CONFIRM_TITLE "$(^NameDA) 제거"
  ${LangFileString} MUI_UNTEXT_CONFIRM_SUBTITLE "$(^NameDA) 제거하기"
!endif

!ifdef MUI_ABORTWARNING
  ${LangFileString} MUI_TEXT_ABORTWARNING "$(^Name) 설치를 취소하시겠습니까?"
!endif

!ifdef MUI_UNABORTWARNING
  ${LangFileString} MUI_UNTEXT_ABORTWARNING "$(^Name) 제거를 취소하시겠습니까?"
!endif
