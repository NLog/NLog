;Language: Polish (1045)
;By Piotr Murawski & Rafa³ Lampe
;Updated by cube and SYSTEMsoft Group
;Updated by Pawe³ Porwisz, http://www.pepesoft.tox.pl
;Corrected by Mateusz Gola (aka Prozac) - http://www.videopedia.pl/avirecomp

!insertmacro LANGFILE "Polish" "Polski"

!ifdef MUI_WELCOMEPAGE
  ${LangFileString} MUI_TEXT_WELCOME_INFO_TITLE "Witamy w kreatorze instalacji programu $(^NameDA)"
  ${LangFileString} MUI_TEXT_WELCOME_INFO_TEXT "Kreator ten pomo¿e Ci zainstalowaæ program $(^NameDA).$\r$\n$\r$\nZalecane jest zamkniêcie wszystkich uruchomionych programów przed rozpoczêciem instalacji. Pozwoli to na uaktualnienie niezbêdnych plików systemowych bez koniecznoœci ponownego uruchamiania komputera.$\r$\n$\r$\n$_CLICK"
!endif

!ifdef MUI_UNWELCOMEPAGE
  ${LangFileString} MUI_UNTEXT_WELCOME_INFO_TITLE "Witamy w kreatorze deinstalacji $(^NameDA)"
  ${LangFileString} MUI_UNTEXT_WELCOME_INFO_TEXT "Kreator poprowadzi Ciê przez proces deinstalacji $(^NameDA).$\r$\n$\r$\nPrzed rozpoczêciem deinstalacji programu, upewnij siê, czy $(^NameDA) NIE jest w³aœnie uruchomiony.$\r$\n$\r$\n$_CLICK"
!endif

!ifdef MUI_LICENSEPAGE
  ${LangFileString} MUI_TEXT_LICENSE_TITLE "Umowa licencyjna"
  ${LangFileString} MUI_TEXT_LICENSE_SUBTITLE "Przed instalacj¹ programu $(^NameDA) zapoznaj siê z warunkami licencji."
  ${LangFileString} MUI_INNERTEXT_LICENSE_BOTTOM "Je¿eli akceptujesz warunki umowy, wybierz Zgadzam siê, aby kontynuowaæ. Musisz zaakceptowaæ warunki umowy, aby zainstalowaæ $(^NameDA)."
  ${LangFileString} MUI_INNERTEXT_LICENSE_BOTTOM_CHECKBOX "Je¿eli akceptujesz warunki umowy, zaznacz pole wyboru poni¿ej, aby kontynuowaæ. Musisz zaakceptowaæ warunki umowy, aby zainstalowaæ $(^NameDA). $_CLICK"
  ${LangFileString} MUI_INNERTEXT_LICENSE_BOTTOM_RADIOBUTTONS "Je¿eli akceptujesz warunki umowy, wybierz pierwsz¹ opcjê poni¿ej, aby kontynuowaæ. Musisz zaakceptowaæ warunki umowy, aby zainstalowaæ $(^NameDA). $_CLICK"
!endif

!ifdef MUI_UNLICENSEPAGE
  ${LangFileString} MUI_UNTEXT_LICENSE_TITLE "Umowa licencyjna"
  ${LangFileString} MUI_UNTEXT_LICENSE_SUBTITLE "Przed deinstalacj¹ programu $(^NameDA) zapoznaj siê z warunkami licencji."
  ${LangFileString} MUI_UNINNERTEXT_LICENSE_BOTTOM "Je¿eli akceptujesz warunki umowy, wybierz Zgadzam siê, aby kontynuowaæ. Musisz zaakceptowaæ warunki umowy, aby odinstalowaæ $(^NameDA)."
  ${LangFileString} MUI_UNINNERTEXT_LICENSE_BOTTOM_CHECKBOX "Je¿eli akceptujesz warunki umowy, zaznacz pole wyboru poni¿ej, aby kontynuowaæ. Musisz zaakceptowaæ warunki umowy, aby odinstalowaæ $(^NameDA). $_CLICK"
  ${LangFileString} MUI_UNINNERTEXT_LICENSE_BOTTOM_RADIOBUTTONS "Je¿eli akceptujesz warunki umowy, wybierz pierwsz¹ opcjê poni¿ej, aby kontynuowaæ. Musisz zaakceptowaæ warunki umowy, aby odinstalowaæ $(^NameDA). $_CLICK"
!endif

!ifdef MUI_LICENSEPAGE | MUI_UNLICENSEPAGE
  ${LangFileString} MUI_INNERTEXT_LICENSE_TOP "Naciœnij klawisz Page Down, aby zobaczyæ resztê umowy."
!endif

!ifdef MUI_COMPONENTSPAGE
  ${LangFileString} MUI_TEXT_COMPONENTS_TITLE "Wybierz komponenty"
  ${LangFileString} MUI_TEXT_COMPONENTS_SUBTITLE "Wybierz komponenty programu $(^NameDA), które chcesz zainstalowaæ."
!endif

!ifdef MUI_UNCOMPONENTSPAGE
  ${LangFileString} MUI_UNTEXT_COMPONENTS_TITLE "Wybierz komponenty"
  ${LangFileString} MUI_UNTEXT_COMPONENTS_SUBTITLE "Wybierz, które elementy $(^NameDA) chcesz odinstalowaæ."
!endif

!ifdef MUI_COMPONENTSPAGE | MUI_UNCOMPONENTSPAGE
  ${LangFileString} MUI_INNERTEXT_COMPONENTS_DESCRIPTION_TITLE "Opis"
  !ifndef NSIS_CONFIG_COMPONENTPAGE_ALTERNATIVE
    ${LangFileString} MUI_INNERTEXT_COMPONENTS_DESCRIPTION_INFO "Przesuñ kursor myszy nad komponent, aby zobaczyæ jego opis."
  !else
    ${LangFileString} MUI_INNERTEXT_COMPONENTS_DESCRIPTION_INFO "Przesuñ kursor myszy nad komponent, aby zobaczyæ jego opis."
  !endif
!endif

!ifdef MUI_DIRECTORYPAGE
  ${LangFileString} MUI_TEXT_DIRECTORY_TITLE "Wybierz lokalizacjê dla instalacji"
  ${LangFileString} MUI_TEXT_DIRECTORY_SUBTITLE "Wybierz folder, w którym ma byæ zainstalowany $(^NameDA)."
!endif

!ifdef MUI_UNDIRECTORYPAGE
  ${LangFileString} MUI_UNTEXT_DIRECTORY_TITLE "Wybór miejsca deinstalacji"
  ${LangFileString} MUI_UNTEXT_DIRECTORY_SUBTITLE "Wybierz folder, z którego chcesz odinstalowaæ $(^NameDA)."
!endif

!ifdef MUI_INSTFILESPAGE
  ${LangFileString} MUI_TEXT_INSTALLING_TITLE "Instalacja"
  ${LangFileString} MUI_TEXT_INSTALLING_SUBTITLE "Proszê czekaæ, podczas gdy $(^NameDA) jest instalowany."
  ${LangFileString} MUI_TEXT_FINISH_TITLE "Zakoñczono instalacjê"
  ${LangFileString} MUI_TEXT_FINISH_SUBTITLE "Instalacja zakoñczona pomyœlnie."
  ${LangFileString} MUI_TEXT_ABORT_TITLE "Instalacja przerwana"
  ${LangFileString} MUI_TEXT_ABORT_SUBTITLE "Instalacja nie zosta³a zakoñczona pomyœlnie."
!endif

!ifdef MUI_UNINSTFILESPAGE
  ${LangFileString} MUI_UNTEXT_UNINSTALLING_TITLE "Deinstalacja"
  ${LangFileString} MUI_UNTEXT_UNINSTALLING_SUBTITLE "Proszê czekaæ, $(^NameDA) jest odinstalowywany."
  ${LangFileString} MUI_UNTEXT_FINISH_TITLE "Zakoñczono odinstalowanie"
  ${LangFileString} MUI_UNTEXT_FINISH_SUBTITLE "Odinstalowanie zakoñczone pomyœlnie."
  ${LangFileString} MUI_UNTEXT_ABORT_TITLE "Deinstalacja przerwana"
  ${LangFileString} MUI_UNTEXT_ABORT_SUBTITLE "Deinstalacja nie zosta³a zakoñczona pomyœlnie."
!endif

!ifdef MUI_FINISHPAGE
  ${LangFileString} MUI_TEXT_FINISH_INFO_TITLE "Koñczenie pracy kreatora instalacji $(^NameDA)"
  ${LangFileString} MUI_TEXT_FINISH_INFO_TEXT "$(^NameDA) zosta³ pomyœlnie zainstalowany na Twoim komputerze.$\r$\n$\r$\nKliknij Zakoñcz, aby zakoñczyæ dzia³anie kreatora."
  ${LangFileString} MUI_TEXT_FINISH_INFO_REBOOT "Twój komputer musi zostaæ ponownie uruchomiony, aby zakoñczyæ instalacjê programu $(^NameDA). Czy chcesz zrobiæ to teraz?"
!endif

!ifdef MUI_UNFINISHPAGE
  ${LangFileString} MUI_UNTEXT_FINISH_INFO_TITLE "Koñczenie pracy kreatora deinstalacji $(^NameDA)"
  ${LangFileString} MUI_UNTEXT_FINISH_INFO_TEXT "$(^NameDA) zosta³ odinstalowany z Twojego komputera.$\r$\n$\r$\nKliknij Zakoñcz, aby zakoñczyæ dzia³anie kreatora."
  ${LangFileString} MUI_UNTEXT_FINISH_INFO_REBOOT "Twój komputer musi zostaæ ponownie uruchomiony w celu zakoñczenia deinstalacji programu $(^NameDA). Czy chcesz zrobiæ to teraz?"
!endif

!ifdef MUI_FINISHPAGE | MUI_UNFINISHPAGE
  ${LangFileString} MUI_TEXT_FINISH_REBOOTNOW "Uruchom ponownie teraz"
  ${LangFileString} MUI_TEXT_FINISH_REBOOTLATER "Sam uruchomiê ponownie komputer póŸniej"
  ${LangFileString} MUI_TEXT_FINISH_RUN "&Uruchom program $(^NameDA)"
  ${LangFileString} MUI_TEXT_FINISH_SHOWREADME "&Poka¿ plik ReadMe"
  ${LangFileString} MUI_BUTTONTEXT_FINISH "&Zakoñcz"  
!endif

!ifdef MUI_STARTMENUPAGE
  ${LangFileString} MUI_TEXT_STARTMENU_TITLE "Wybierz folder w menu Start"
  ${LangFileString} MUI_TEXT_STARTMENU_SUBTITLE "Wybierz folder menu Start, w którym zostan¹ umieszczone skróty do programu"
  ${LangFileString} MUI_INNERTEXT_STARTMENU_TOP "Wybierz folder w menu Start, w którym chcia³byœ umieœciæ skróty do programu. Mo¿esz tak¿e utworzyæ nowy folder wpisuj¹c jego nazwê."
  ${LangFileString} MUI_INNERTEXT_STARTMENU_CHECKBOX "Nie twórz skrótów"
!endif

!ifdef MUI_UNCONFIRMPAGE
  ${LangFileString} MUI_UNTEXT_CONFIRM_TITLE "Odinstaluj $(^NameDA)"
  ${LangFileString} MUI_UNTEXT_CONFIRM_SUBTITLE "Usuñ $(^NameDA) z Twojego komputera."
!endif

!ifdef MUI_ABORTWARNING
  ${LangFileString} MUI_TEXT_ABORTWARNING "Czy na pewno chcesz zakoñczyæ dzia³anie instalatora $(^Name)?"
!endif

!ifdef MUI_UNABORTWARNING
  ${LangFileString} MUI_UNTEXT_ABORTWARNING "Czy na pewno chcesz przerwaæ proces deinstalacji $(^Name)?"
!endif

!ifdef MULTIUSER_INSTALLMODEPAGE
  ${LangFileString} MULTIUSER_TEXT_INSTALLMODE_TITLE "Wybierz u¿ytkowników"
  ${LangFileString} MULTIUSER_TEXT_INSTALLMODE_SUBTITLE "Wybierz, dla których u¿ytkowników chcesz zainstalowaæ program $(^NameDA)."
  ${LangFileString} MULTIUSER_INNERTEXT_INSTALLMODE_TOP "Wybierz, czy chcesz zainstalowaæ program $(^NameDA) tylko dla siebie, czy dla wszystkich u¿ytkowników tego komputera. $(^ClickNext)"
  ${LangFileString} MULTIUSER_INNERTEXT_INSTALLMODE_ALLUSERS "Zainstaluj dla wszystkich u¿ytkowników tego komputera"
  ${LangFileString} MULTIUSER_INNERTEXT_INSTALLMODE_CURRENTUSER "Zainstaluj tylko dla mnie"
!endif
