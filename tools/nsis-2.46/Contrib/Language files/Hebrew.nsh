;Language: Hebrew (1037)
;By Yaron Shahrabani

!insertmacro LANGFILE "Hebrew" "Hebrew"

!ifdef MUI_WELCOMEPAGE
  ${LangFileString} MUI_TEXT_WELCOME_INFO_TITLE "ברוכים הבאים לאשף ההתקנה של $(^NameDA)"
  ${LangFileString} MUI_TEXT_WELCOME_INFO_TEXT "אשף זה ינחה אתכם במהלך ההתקנה של $(^NameDA).$\r$\n$\r$\nמומלץ לסגור כל תוכנית אחרת לפני התחלת ההתקנה. פעולה זו תאפשר לאשף לעדכן קבצי מערכת ללא איתחול המחשב.$\r$\n$\r$\n$_CLICK"
!endif

!ifdef MUI_UNWELCOMEPAGE
  ${LangFileString} MUI_UNTEXT_WELCOME_INFO_TITLE "ברוכים הבאים לאשף ההסרה של $(^NameDA)"
  ${LangFileString} MUI_UNTEXT_WELCOME_INFO_TEXT "אשף זה ינחה אתכם במהלך ההסרה של $(^NameDA).$\r$\n$\r$\nמומלץ לסגור כל תוכנית אחרת לפני התחלת ההסרה. פעולה זו תאפשר לאשף לעדכן קבצי מערכת ללא איתחול המחשב.$\r$\n$\r$\n$_CLICK"
!endif

!ifdef MUI_LICENSEPAGE
  ${LangFileString} MUI_TEXT_LICENSE_TITLE "הסכם רישוי"
  ${LangFileString} MUI_TEXT_LICENSE_SUBTITLE "נא עיין בתנאי הסכם הרישוי לפני התקנת $(^NameDA)."
  ${LangFileString} MUI_INNERTEXT_LICENSE_BOTTOM "אם אתה מקבל את תנאי ההסכם, לחץ על 'אני מסכים' כדי להמשיך. אם לא תסכים לתנאי ההסכם לא תוכל להתקין את $(^NameDA)."
  ${LangFileString} MUI_INNERTEXT_LICENSE_BOTTOM_CHECKBOX "אם אתה מקבל את תנאי ההסכם, סמן את תיבת הבחירה שלהלן. עלייך לקבל את תנאי ההסכם בכדי להתקין את $(^NameDA). $_CLICK"
  ${LangFileString} MUI_INNERTEXT_LICENSE_BOTTOM_RADIOBUTTONS "אם אתה מקבל את תנאי ההסכם, בחר באפשרות הראשונה שלהלן. עלייך לקבל את ההסכם כדי להתקין את $(^NameDA). $_CLICK"
!endif

!ifdef MUI_UNLICENSEPAGE
  ${LangFileString} MUI_UNTEXT_LICENSE_TITLE "הסכם רישוי"
  ${LangFileString} MUI_UNTEXT_LICENSE_SUBTITLE "נא עיין בתנאי הסכם הרישוי לפני הסרת $(^NameDA)."
  ${LangFileString} MUI_UNINNERTEXT_LICENSE_BOTTOM "אם אתה מקבל את תנאי ההסכם, לחץ על 'אני מסכים' כדי להמשיך. אם לא תקבל את תנאי ההסכם לא תוכל להסיר את $(^NameDA)."
  ${LangFileString} MUI_UNINNERTEXT_LICENSE_BOTTOM_CHECKBOX "אם אתה מקבל את תנאי ההסכם, סמן את תיבת הבחירה שלהלן. עלייך לקבל את תנאי ההסכם כדי להסיר את $(^NameDA). $_CLICK"
  ${LangFileString} MUI_UNINNERTEXT_LICENSE_BOTTOM_RADIOBUTTONS "אם אתה מקבל את תנאי ההסכם, בחר באפשרות הראשונה שלהלן. עלייך לקבל את ההסכם כדי להסיר את $(^NameDA). $_CLICK"
!endif

!ifdef MUI_LICENSEPAGE | MUI_UNLICENSEPAGE
  ${LangFileString} MUI_INNERTEXT_LICENSE_TOP "כדי לצפות בשאר הסכם הרישוי לחץ על Page Down."
!endif

!ifdef MUI_COMPONENTSPAGE
  ${LangFileString} MUI_TEXT_COMPONENTS_TITLE "בחר רכיבים"
  ${LangFileString} MUI_TEXT_COMPONENTS_SUBTITLE "בחר אילו רכיבים של $(^NameDA) ברצונך להתקין."
!endif

!ifdef MUI_UNCOMPONENTSPAGE
  ${LangFileString} MUI_UNTEXT_COMPONENTS_TITLE "בחר רכיבים"
  ${LangFileString} MUI_UNTEXT_COMPONENTS_SUBTITLE "בחר אילו תכונות של $(^NameDA) ברצונך להסיר."
!endif

!ifdef MUI_COMPONENTSPAGE | MUI_UNCOMPONENTSPAGE
  ${LangFileString} MUI_INNERTEXT_COMPONENTS_DESCRIPTION_TITLE "תיאור"
  !ifndef NSIS_CONFIG_COMPONENTPAGE_ALTERNATIVE
    ${LangFileString} MUI_INNERTEXT_COMPONENTS_DESCRIPTION_INFO "העבר את העכבר מעל רכיב כלשהו בכדי לצפות בתיאורו."
  !else
    ${LangFileString} MUI_INNERTEXT_COMPONENTS_DESCRIPTION_INFO "העבר את העכבר מעל רכיב כלשהו בכדי לצפות בתיאורו."
  !endif
!endif

!ifdef MUI_DIRECTORYPAGE
  ${LangFileString} MUI_TEXT_DIRECTORY_TITLE "בחר מיקום להתקנה"
  ${LangFileString} MUI_TEXT_DIRECTORY_SUBTITLE "בחר את התיקייה בה אתה מעוניין להתקין את $(^NameDA)."
!endif

!ifdef MUI_UNDIRECTORYPAGE
  ${LangFileString} MUI_UNTEXT_DIRECTORY_TITLE "בחר מיקום להסרה"
  ${LangFileString} MUI_UNTEXT_DIRECTORY_SUBTITLE "בחר את התיקייה ממנה אתה מעוניין להסיר את $(^NameDA)."
!endif

!ifdef MUI_INSTFILESPAGE
  ${LangFileString} MUI_TEXT_INSTALLING_TITLE "מתקין"
  ${LangFileString} MUI_TEXT_INSTALLING_SUBTITLE "אנא המתן בזמן ש-$(^NameDA) מותקן."
  ${LangFileString} MUI_TEXT_FINISH_TITLE "ההתקנה הושלמה"
  ${LangFileString} MUI_TEXT_FINISH_SUBTITLE "ההתקנה הושלמה במלואה."
  ${LangFileString} MUI_TEXT_ABORT_TITLE "ההתקנה בוטלה"
  ${LangFileString} MUI_TEXT_ABORT_SUBTITLE "ההתקנה לא הושלמה המלואה."
!endif

!ifdef MUI_UNINSTFILESPAGE
  ${LangFileString} MUI_UNTEXT_UNINSTALLING_TITLE "מסיר"
  ${LangFileString} MUI_UNTEXT_UNINSTALLING_SUBTITLE "אנא המתן בזמן ש-$(^NameDA) מוסר מהמחשב."
  ${LangFileString} MUI_UNTEXT_FINISH_TITLE "ההסרה הושלמה"
  ${LangFileString} MUI_UNTEXT_FINISH_SUBTITLE "ההסרה הושלמה במלואה."
  ${LangFileString} MUI_UNTEXT_ABORT_TITLE "ההסרה בוטלה"
  ${LangFileString} MUI_UNTEXT_ABORT_SUBTITLE "ההסרה לא הושלמה במלואה."
!endif

!ifdef MUI_FINISHPAGE
  ${LangFileString} MUI_TEXT_FINISH_INFO_TITLE "משלים את אשף ההתקנה של $(^NameDA)"
  ${LangFileString} MUI_TEXT_FINISH_INFO_TEXT "$(^NameDA) הותקן בהצלחה.$\r$\n$\r$\nלחץ על סיום כדי לסגור את האשף."
  ${LangFileString} MUI_TEXT_FINISH_INFO_REBOOT "עלייך לאתחל את המחשב כדי לסיים את התקנת $(^NameDA). האם ברצונך לאתחל כעת?"
!endif

!ifdef MUI_UNFINISHPAGE
  ${LangFileString} MUI_UNTEXT_FINISH_INFO_TITLE "משלים את אשף ההסרה של $(^NameDA)"
  ${LangFileString} MUI_UNTEXT_FINISH_INFO_TEXT "$(^NameDA) הוסר בהצלחה.$\r$\n$\r$\nלחץ על סיום כדי לסגור את האשף."
  ${LangFileString} MUI_UNTEXT_FINISH_INFO_REBOOT "עלייך לאתחל את המחשב כדי לסיים את הסרת $(^NameDA). האם ברצונך לאתחל כעת?"
!endif

!ifdef MUI_FINISHPAGE | MUI_UNFINISHPAGE
  ${LangFileString} MUI_TEXT_FINISH_REBOOTNOW "אתחל כעת"
  ${LangFileString} MUI_TEXT_FINISH_REBOOTLATER "ברצוני לאתחל ידנית מאוחר יותר"
  ${LangFileString} MUI_TEXT_FINISH_RUN "&הרץ את $(^NameDA)"
  ${LangFileString} MUI_TEXT_FINISH_SHOWREADME "&הצג מסמך 'קרא אותי'"
  ${LangFileString} MUI_BUTTONTEXT_FINISH "&סיים"  
!endif

!ifdef MUI_STARTMENUPAGE
  ${LangFileString} MUI_TEXT_STARTMENU_TITLE "בחר תיקייה בתפריט ההתחלה"
  ${LangFileString} MUI_TEXT_STARTMENU_SUBTITLE "בחר בתיקיית תפריט ההתחלה בה יווצרו קיצורי הדרך של התוכנית."
  ${LangFileString} MUI_INNERTEXT_STARTMENU_TOP "בחר בתיקייה מתפריט ההתחלה בה ברצונך ליצור את קיצורי הדרך עבור התוכנית. באפשרותך גם להקליד את שם התיקייה כדי ליצור תיקייה חדשה."
  ${LangFileString} MUI_INNERTEXT_STARTMENU_CHECKBOX "אל תיצור קיצורי דרך"
!endif

!ifdef MUI_UNCONFIRMPAGE
  ${LangFileString} MUI_UNTEXT_CONFIRM_TITLE "הסר את $(^NameDA)"
  ${LangFileString} MUI_UNTEXT_CONFIRM_SUBTITLE "הסר את $(^NameDA) מהמחשב."
!endif

!ifdef MUI_ABORTWARNING
  ${LangFileString} MUI_TEXT_ABORTWARNING "האם אתה בטוח שברצונך לצאת מהתקנת $(^Name)?"
!endif

!ifdef MUI_UNABORTWARNING
  ${LangFileString} MUI_UNTEXT_ABORTWARNING "האם אתה בטוח שברצונך לצאת מהסרת $(^Name)?"
!endif

!ifdef MULTIUSER_INSTALLMODEPAGE
  ${LangFileString} MULTIUSER_TEXT_INSTALLMODE_TITLE "בחר משתמשים"
  ${LangFileString} MULTIUSER_TEXT_INSTALLMODE_SUBTITLE "בחר לאילו משתמשים להתקין את $(^NameDA)."
  ${LangFileString} MULTIUSER_INNERTEXT_INSTALLMODE_TOP "בחר האם להתקין את $(^NameDA) לעצמך או לכל המשתמשים של המחשב. $(^ClickNext)"
  ${LangFileString} MULTIUSER_INNERTEXT_INSTALLMODE_ALLUSERS "התקן לכל משתמשי המחשב"
  ${LangFileString} MULTIUSER_INNERTEXT_INSTALLMODE_CURRENTUSER "התקן רק למשתמש שלי"
!endif
