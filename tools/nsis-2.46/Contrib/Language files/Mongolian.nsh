;Language: Mongolian (1104)
;By Bayarsaikhan Enkhtaivan

!insertmacro LANGFILE "Mongolian" "Mongolian"

!ifdef MUI_WELCOMEPAGE
  ${LangFileString} MUI_TEXT_WELCOME_INFO_TITLE "$(^NameDA) Суулгацад тавтай морил"
  ${LangFileString} MUI_TEXT_WELCOME_INFO_TEXT "$(^NameDA) суулгацын илбэчинг та шууд ашиглаж болно.$\r$\n$\r$\nЇїнийг суулгахын ємнє бусад бїх програмуудаа хаахыг зєвлєж байна. Системийн файлуудыг шинэчилбэл компьютерээ дахин ачаалахгїй байх боломжтой.$\r$\n$\r$\n$_CLICK"
!endif

!ifdef MUI_UNWELCOMEPAGE
  ${LangFileString} MUI_UNTEXT_WELCOME_INFO_TITLE "$(^NameDA) Суулгацыг устгах илбэчинд тавтай морил"
  ${LangFileString} MUI_UNTEXT_WELCOME_INFO_TEXT "$(^NameDA) устгацын илбэчинг та шууд ашиглаж болно.$\r$\n$\r$\nУстгахын ємнє $(^NameDA) нь ажиллаагїй эсэхийг шалга.$\r$\n$\r$\n$_CLICK"
!endif

!ifdef MUI_LICENSEPAGE
  ${LangFileString} MUI_TEXT_LICENSE_TITLE "Лицензийн зєвшєєрєл"
  ${LangFileString} MUI_TEXT_LICENSE_SUBTITLE "$(^NameDA)-ыг суулгахынхаа ємнє зєвшилцлийн зїйлїїдийг уншина уу."
  ${LangFileString} MUI_INNERTEXT_LICENSE_BOTTOM "Хэрэв зєвшилцлийн зїйлсийг зєвшєєрч байвал, Зєвшєєрлєє товчийг даран їргэлжлїїлнэ її. $(^NameDA)-ыг суулгахын тулд заавал зєвшєєрєх шаардлагатай."
  ${LangFileString} MUI_INNERTEXT_LICENSE_BOTTOM_CHECKBOX "Хэрэв зєвшилцлийн зїйлсийг зєвшєєрч байвал, Зєвлєх хайрцгийг даран їргэлжлїїлнэ її. $(^NameDA)-ыг суулгахын тулд заавал зєвшєєрєх шаардлагатай. $_CLICK"
  ${LangFileString} MUI_INNERTEXT_LICENSE_BOTTOM_RADIOBUTTONS "Хэрэв зєвшилцлийн зїйлсийг зєвшєєрч байвал, доорхоос эхнийг нь сонгон їргэлжлїїлнэ її. $(^NameDA)-ыг суулгахын тулд заавал зєвшєєрєх шаардлагатай. $_CLICK"
!endif

!ifdef MUI_UNLICENSEPAGE
  ${LangFileString} MUI_UNTEXT_LICENSE_TITLE "Лицензийн зєвшєєрєл"
  ${LangFileString} MUI_UNTEXT_LICENSE_SUBTITLE "$(^NameDA) устгахын ємнє зєвшилцлийн зїйлсийг уншина уу."
  ${LangFileString} MUI_UNINNERTEXT_LICENSE_BOTTOM "Хэрэв зєвшилцлийн зїйлсийг зєвшєєрч байвал, Зєвшєєрлєє товчийг даран їргэлжлїїлнэ її. $(^NameDA)-ыг устгахын тулд заавал зєвшєєрєх шаардлагатай."
  ${LangFileString} MUI_UNINNERTEXT_LICENSE_BOTTOM_CHECKBOX "Хэрэв зєвшилцлийн зїйлсийг зєвшєєрч байвал, Зєвлєх хайрцгийг даран їргэлжлїїлнэ її. $(^NameDA)-ыг устгахын тулд заавал зєвшєєрєх шаардлагатай. $_CLICK"
  ${LangFileString} MUI_UNINNERTEXT_LICENSE_BOTTOM_RADIOBUTTONS "Хэрэв зєвшилцлийн зїйлсийг зєвшєєрч байвал, доорхоос эхнийг нь сонгон їргэлжлїїлнэ її. $(^NameDA)-ыг устгахын тулд заавал зєвшєєрєх шаардлагатай. $_CLICK"
!endif

!ifdef MUI_LICENSEPAGE | MUI_UNLICENSEPAGE
  ${LangFileString} MUI_INNERTEXT_LICENSE_TOP "Page Down товчийг даран зєвшилцлийг доош гїйлгэнэ її."
!endif

!ifdef MUI_COMPONENTSPAGE
  ${LangFileString} MUI_TEXT_COMPONENTS_TITLE "Нэгдлийг сонгох"
  ${LangFileString} MUI_TEXT_COMPONENTS_SUBTITLE "$(^NameDA)-ыг суулгахад шаардагдах хэсгийг сонгоно уу."
!endif

!ifdef MUI_UNCOMPONENTSPAGE
  ${LangFileString} MUI_UNTEXT_COMPONENTS_TITLE "Нэгдлийг сонгох"
  ${LangFileString} MUI_UNTEXT_COMPONENTS_SUBTITLE "$(^NameDA)-ын устгах шаардлагатай нэгдлийг сонгох."
!endif

!ifdef MUI_COMPONENTSPAGE | MUI_UNCOMPONENTSPAGE
  ${LangFileString} MUI_INNERTEXT_COMPONENTS_DESCRIPTION_TITLE "Тайлбар"
  !ifndef NSIS_CONFIG_COMPONENTPAGE_ALTERNATIVE
    ${LangFileString} MUI_INNERTEXT_COMPONENTS_DESCRIPTION_INFO "Та хулганаараа нэгдлийн дээр очиход тїїний тайлбарыг харуулна."
  !else
    ${LangFileString} MUI_INNERTEXT_COMPONENTS_DESCRIPTION_INFO "Та хулганаараа нэгдлийн дээр очиход тїїний тайлбарыг харуулна."
  !endif
!endif

!ifdef MUI_DIRECTORYPAGE
  ${LangFileString} MUI_TEXT_DIRECTORY_TITLE "Суулгах байрлалыг сонгох"
  ${LangFileString} MUI_TEXT_DIRECTORY_SUBTITLE "$(^NameDA) суулгацын суулгах замыг сонго."
!endif

!ifdef MUI_UNDIRECTORYPAGE
  ${LangFileString} MUI_UNTEXT_DIRECTORY_TITLE "Устгацын байрлалыг сонгох"
  ${LangFileString} MUI_UNTEXT_DIRECTORY_SUBTITLE "$(^NameDA)-ыг устгах хавтсыг сонгох."
!endif

!ifdef MUI_INSTFILESPAGE
  ${LangFileString} MUI_TEXT_INSTALLING_TITLE "Суулгаж байна"
  ${LangFileString} MUI_TEXT_INSTALLING_SUBTITLE "$(^NameDA)-ыг суулгаж дуустал тїр хїлээнэ її."
  ${LangFileString} MUI_TEXT_FINISH_TITLE "Суулгаж дууслаа"
  ${LangFileString} MUI_TEXT_FINISH_SUBTITLE "Суулгац амжилттай болов."
  ${LangFileString} MUI_TEXT_ABORT_TITLE "Суулгалт таслагдлаа"
  ${LangFileString} MUI_TEXT_ABORT_SUBTITLE "Суулгалт амжилтгїй болов."
!endif

!ifdef MUI_UNINSTFILESPAGE
  ${LangFileString} MUI_UNTEXT_UNINSTALLING_TITLE "Устгаж байна"
  ${LangFileString} MUI_UNTEXT_UNINSTALLING_SUBTITLE "$(^NameDA) -ыг зайлуулж дуустал тїр хїлээнэ її."
  ${LangFileString} MUI_UNTEXT_FINISH_TITLE "Устгаж дууслаа"
  ${LangFileString} MUI_UNTEXT_FINISH_SUBTITLE "Устгалт амжилттай дууслаа."
  ${LangFileString} MUI_UNTEXT_ABORT_TITLE "Устгац таслагдлаа"
  ${LangFileString} MUI_UNTEXT_ABORT_SUBTITLE "Устгалт амжилтгїй боллоо."
!endif

!ifdef MUI_FINISHPAGE
  ${LangFileString} MUI_TEXT_FINISH_INFO_TITLE "$(^NameDA) Суулгацын илбэчин дууслаа"
  ${LangFileString} MUI_TEXT_FINISH_INFO_TEXT "$(^NameDA) нь таны компьютерт суулаа.$\r$\n$\r$\nТєгсгєл дээр дарвал хаана."
  ${LangFileString} MUI_TEXT_FINISH_INFO_REBOOT "$(^NameDA)-ын суулгацын дараалалд та компьютерээ дахин ачаалснаар дуусна. Та дахин ачаалахыг хїсэж байна уу?"
!endif

!ifdef MUI_UNFINISHPAGE
  ${LangFileString} MUI_UNTEXT_FINISH_INFO_TITLE "$(^NameDA) Устгацын илбэчин дууслаа"
  ${LangFileString} MUI_UNTEXT_FINISH_INFO_TEXT "$(^NameDA) нь таны компьютерээс зайлуулагдлаа.$\r$\n$\r$\nТєгсгєл дээр дарвал хаана."
  ${LangFileString} MUI_UNTEXT_FINISH_INFO_REBOOT "$(^NameDA) Устгацын дараалалд та компьютерээ дахин ачаалснаар дуусна. Та д.ачаалмаар байна уу?"
!endif

!ifdef MUI_FINISHPAGE | MUI_UNFINISHPAGE
  ${LangFileString} MUI_TEXT_FINISH_REBOOTNOW "Д.Ачаал"
  ${LangFileString} MUI_TEXT_FINISH_REBOOTLATER "Би дараа д.ачаалахыг хїсэж байна."
  ${LangFileString} MUI_TEXT_FINISH_RUN "$(^NameDA) ажиллуулах"
  ${LangFileString} MUI_TEXT_FINISH_SHOWREADME "&Readme харуулах"
  ${LangFileString} MUI_BUTTONTEXT_FINISH "&Тєгсгєл"  
!endif

!ifdef MUI_STARTMENUPAGE
  ${LangFileString} MUI_TEXT_STARTMENU_TITLE "Start цэсний хавтсыг сонго"
  ${LangFileString} MUI_TEXT_STARTMENU_SUBTITLE "Start цэс дэх $(^NameDA) shortcut-ын хавтсыг сонго."
  ${LangFileString} MUI_INNERTEXT_STARTMENU_TOP "Start цэсэнд програмын shortcut їїсгэх хавтсыг сонго. Эсвэл та шинэ нэрээр їїсгэж болно."
  ${LangFileString} MUI_INNERTEXT_STARTMENU_CHECKBOX "Do not create shortcuts"
!endif

!ifdef MUI_UNCONFIRMPAGE
  ${LangFileString} MUI_UNTEXT_CONFIRM_TITLE "$(^NameDA)--ын Устгац"
  ${LangFileString} MUI_UNTEXT_CONFIRM_SUBTITLE "$(^NameDA) -ыг таны компьютерээс зайлуулах."
!endif

!ifdef MUI_ABORTWARNING
  ${LangFileString} MUI_TEXT_ABORTWARNING "$(^Name) -ын суулгацаас гармаар байна уу?"
!endif

!ifdef MUI_UNABORTWARNING
  ${LangFileString} MUI_UNTEXT_ABORTWARNING "$(^Name) Устгацаас гармаар байна уу?"
!endif
