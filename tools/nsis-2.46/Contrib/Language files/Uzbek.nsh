;Language: Uzbek (1091)
;Translation updated by Emil Garipov [emil.garipov@gmail.com] 

!insertmacro LANGFILE "Uzbek" "Uzbek"

!ifdef MUI_WELCOMEPAGE
  ${LangFileString} MUI_TEXT_WELCOME_INFO_TITLE "Sizni o'rnatish dastur tabriklaydi $(^NameDA)"
  ${LangFileString} MUI_TEXT_WELCOME_INFO_TEXT "Bu dastur sizning komputeringizga $(^NameDA) dasturni o'rnatadi.$\r$\n$\r$\nO'rnatishdan oldin ishlayotgan barcha ilovalarni yopish tavsiya etiladi. Bu o'rnatuvchi dasturga kompyuterni qayta yuklamasdan sistemali fayllarni yangilash imkonini beradi.$\r$\n$\r$\n$_CLICK"
!endif

!ifdef MUI_UNWELCOMEPAGE
  ${LangFileString} MUI_UNTEXT_WELCOME_INFO_TITLE "Sizni $(^NameDA)ni o'chirish dasturi tabriklaydi"
  ${LangFileString} MUI_UNTEXT_WELCOME_INFO_TEXT "Bu dastur $(^NameDA)ni sizning kompyuteringizdan o'chiradi.$\r$\n$\r$\nO'chirishdan oldin $(^NameDA) dasturni ishlamayotganligini aniqlang.$\r$\n$\r$\n$_CLICK"
!endif

!ifdef MUI_LICENSEPAGE
  ${LangFileString} MUI_TEXT_LICENSE_TITLE "Lisenzion kelishuv"
  ${LangFileString} MUI_TEXT_LICENSE_SUBTITLE "$(^NameDA) dasturini o'rnatishdan oldin lisenzion kelishuv bilan tanishib chiking."
  ${LangFileString} MUI_INNERTEXT_LICENSE_BOTTOM "Agar kelishuv shartlariga rozi bo'lsangiz $\"Qabul kilaman$\" tugmasini bosing.Dasturni o'rnatish uchun,kelishuv shartlarini qabul qilish kerak."
  ${LangFileString} MUI_INNERTEXT_LICENSE_BOTTOM_CHECKBOX "Agar siz kelishuv shartlarini qabul kilsangiz,bayroqchani joylashtiring. Dasturni o'rnatish uchun kelisuv shartlarini qabul qilish kerak. $_CLICK"
  ${LangFileString} MUI_INNERTEXT_LICENSE_BOTTOM_RADIOBUTTONS "Kelishuv shartlarini qabul qilsangiz quida taklif etilganlardan birinchi variantni tanlang. Dasturni o'rnatish uchun kelisuv shartlarini qabul qilish kerak. $_CLICK"
!endif

!ifdef MUI_UNLICENSEPAGE
  ${LangFileString} MUI_UNTEXT_LICENSE_TITLE "Lisenzion kelishuv"
  ${LangFileString} MUI_UNTEXT_LICENSE_SUBTITLE "$(^NameDA)ni o'chirishdan oldin lesinzion kelishuv bilan tanishing."
  ${LangFileString} MUI_UNINNERTEXT_LICENSE_BOTTOM "Agar siz kelishuv shartlariniqabul qilsangiz $\"Qabul qilaman$\" tugmasini bosing. O'chirish uchun kelishuv shartlarini qabul qilishingiz kerak. $_CLICK"
  ${LangFileString} MUI_UNINNERTEXT_LICENSE_BOTTOM_CHECKBOX "Agar shartlarni qabul qilsangiz, bayroqchani o'rnating.O'chirish uchun kelishuv shartlarini qabul qilishingiz kerak. $_CLICK"
  ${LangFileString} MUI_UNINNERTEXT_LICENSE_BOTTOM_RADIOBUTTONS "Kelishuv shartlarini qabul qilsangiz, taklif etilganlardan birinchi variantni tanlang.O'chirish uchun kelishuv shartlarini qabul qilishingiz kerak. $_CLICK"
!endif

!ifdef MUI_LICENSEPAGE | MUI_UNLICENSEPAGE
  ${LangFileString} MUI_INNERTEXT_LICENSE_TOP "Matn bo'icha silgish uchun $\"PageUp$\" va $\"PageDown$\" tugmasidan foydalaning."
!endif

!ifdef MUI_COMPONENTSPAGE
  ${LangFileString} MUI_TEXT_COMPONENTS_TITLE "O'rnatilayotgan dastur komponentlari"
  ${LangFileString} MUI_TEXT_COMPONENTS_SUBTITLE "$(^NameDA) dasturning o'zingizga kerak bo'lgan komponentasini tanlang."
!endif

!ifdef MUI_UNCOMPONENTSPAGE
  ${LangFileString} MUI_UNTEXT_COMPONENTS_TITLE "Dastur komponentlari"
  ${LangFileString} MUI_UNTEXT_COMPONENTS_SUBTITLE "$(^NameDA)ning o'chirish kerak bo'lgan komponentlarini tanlang."
!endif

!ifdef MUI_COMPONENTSPAGE | MUI_UNCOMPONENTSPAGE
  ${LangFileString} MUI_INNERTEXT_COMPONENTS_DESCRIPTION_TITLE "Tasvir"
  !ifndef NSIS_CONFIG_COMPONENTPAGE_ALTERNATIVE
    ${LangFileString} MUI_INNERTEXT_COMPONENTS_DESCRIPTION_INFO "Sichqonchaning kursorini komponent tasvirini o'qish uchun ustiga quying."
  !else
    ${LangFileString} MUI_INNERTEXT_COMPONENTS_DESCRIPTION_INFO "Sichqonchaning kursorini komponent tasvirini o'qish uchun ustiga quying."
  !endif
!endif

!ifdef MUI_DIRECTORYPAGE
  ${LangFileString} MUI_TEXT_DIRECTORY_TITLE "O'rnatish papkasini tanlash"
  ${LangFileString} MUI_TEXT_DIRECTORY_SUBTITLE "$(^NameDA)ni o'rnatish uchun papka tanlang."
!endif

!ifdef MUI_UNDIRECTORYPAGE
  ${LangFileString} MUI_UNTEXT_DIRECTORY_TITLE "O'chiriladigan papkani tanlash"
  ${LangFileString} MUI_UNTEXT_DIRECTORY_SUBTITLE "$(^NameDA) o'chiriladigan papkasini ko'rsating."
!endif

!ifdef MUI_INSTFILESPAGE
  ${LangFileString} MUI_TEXT_INSTALLING_TITLE "Fayllarni ko'chirish"
  ${LangFileString} MUI_TEXT_INSTALLING_SUBTITLE "Biror kuting, $(^NameDA) fayllari ko'chirilmoqda..."
  ${LangFileString} MUI_TEXT_FINISH_TITLE "O'rnatish jarayoni tugadi"
  ${LangFileString} MUI_TEXT_FINISH_SUBTITLE "O'rnatish jarayoni muvaffaqiyat bilan tugadi."
  ${LangFileString} MUI_TEXT_ABORT_TITLE "O'rnatish jarayoni uzildi"
  ${LangFileString} MUI_TEXT_ABORT_SUBTITLE "O'rnatish jarayoni tugamadi."
!endif

!ifdef MUI_UNINSTFILESPAGE
  ${LangFileString} MUI_UNTEXT_UNINSTALLING_TITLE "O'chirish"
  ${LangFileString} MUI_UNTEXT_UNINSTALLING_SUBTITLE "Biror kutib turing, $(^NameDA) fayllarini o'chirish bajarilmoqda..."
  ${LangFileString} MUI_UNTEXT_FINISH_TITLE "O'chirish tuganlandi"
  ${LangFileString} MUI_UNTEXT_FINISH_SUBTITLE "Dasturni o'chirish muvaffaqiyatli yakunlandi."
  ${LangFileString} MUI_UNTEXT_ABORT_TITLE "O'chirish jarayoni uzildi"
  ${LangFileString} MUI_UNTEXT_ABORT_SUBTITLE "O'chirish to'la bajarilmadi."
!endif

!ifdef MUI_FINISHPAGE
  ${LangFileString} MUI_TEXT_FINISH_INFO_TITLE "$(^NameDA)ni o'rnatuvci dasturi o'z ishini tugatmoqda"
  ${LangFileString} MUI_TEXT_FINISH_INFO_TEXT "$(^NameDA)ni o'rnatish bajarildi.$\r$\n$\r$\nO'rnatuvchi dasturdan chiqish uchun $\"Tayor$\" tugmasini bosing."
  ${LangFileString} MUI_TEXT_FINISH_INFO_REBOOT "$(^NameDA) dasturini o'rnatish jarayonini tugatish uchun Kompyuterni qayta yuklash kerak.Shu ishni bajarishni xoziroq istaysizmi?"
!endif

!ifdef MUI_UNFINISHPAGE
  ${LangFileString} MUI_UNTEXT_FINISH_INFO_TITLE "$(^NameDA)ni o'chirish dasturi o'z ishini tugatdi."
  ${LangFileString} MUI_UNTEXT_FINISH_INFO_TEXT "$(^NameDA) dasturi kompyuteringizdan o'chirildi.$\r$\n$\r$\nO'chirish dasturidan chiqish uchun $\"Tayor$\"tugmasini bosing."
  ${LangFileString} MUI_UNTEXT_FINISH_INFO_REBOOT "$(^NameDA) dasturini o'chirishni tugatish uchun kompyuterni qayta yuklash kerak.shu ishni xozir bajarasizmi?"
!endif

!ifdef MUI_FINISHPAGE | MUI_UNFINISHPAGE
  ${LangFileString} MUI_TEXT_FINISH_REBOOTNOW "Ha, kompyuter hozir qayta yuklansin"
  ${LangFileString} MUI_TEXT_FINISH_REBOOTLATER "Yo'q, bu ishni keyinroq bajaraman"
  ${LangFileString} MUI_TEXT_FINISH_RUN "$(^NameDA) &Ishga tushirilsin"
  ${LangFileString} MUI_TEXT_FINISH_SHOWREADME "&Readme fayli ko'rsatilsin"
  ${LangFileString} MUI_BUTTONTEXT_FINISH "&Tayor"  
!endif

!ifdef MUI_STARTMENUPAGE
  ${LangFileString} MUI_TEXT_STARTMENU_TITLE "Papka $\"Пуск$\" menyusida"
  ${LangFileString} MUI_TEXT_STARTMENU_SUBTITLE "Dastur belgilarini joylashtirish uchun $\"Пуск$\" menyusidan papka tanlang."
  ${LangFileString} MUI_INNERTEXT_STARTMENU_TOP "$\"Пуск$\" menyusidan dastur belgilari joylashadigan papka tanlang. Siz papkaning boshqa ismini kiritishingiz mumkin"
  ${LangFileString} MUI_INNERTEXT_STARTMENU_CHECKBOX "Belgilar yaratilmasin"
!endif

!ifdef MUI_UNCONFIRMPAGE
  ${LangFileString} MUI_UNTEXT_CONFIRM_TITLE "$(^NameDA)ni o'chirish"
  ${LangFileString} MUI_UNTEXT_CONFIRM_SUBTITLE "$(^NameDA)ni kompyuterdan o'chirish."
!endif

!ifdef MUI_ABORTWARNING
  ${LangFileString} MUI_TEXT_ABORTWARNING "Haqiqatdan ham siz $(^Name)ni o'rnatishni bekor qilmoqchimisiz?"
!endif

!ifdef MUI_UNABORTWARNING
  ${LangFileString} MUI_UNTEXT_ABORTWARNING "$(^Name)ni o'chirish jarayonini bekor qilmoqchisizmi?"
!endif
