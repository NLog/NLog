;Language: Turkish (1055)
;By Çagatay Dilsiz(Chagy)
;Updated by Fatih BOY (fatih_boy@yahoo.com)

!insertmacro LANGFILE "Turkish" "Türkçe"

!ifdef MUI_WELCOMEPAGE
  ${LangFileString} MUI_TEXT_WELCOME_INFO_TITLE "$(^NameDA) Kurulum sihirbazýna hoþ geldiniz"
  ${LangFileString} MUI_TEXT_WELCOME_INFO_TEXT "Bu sihirbaz size $(^NameDA) kurulumu boyunca rehberlik edecektir.$\r$\n$\r$\nKurulumu baþlatmadan önce çalýþan diðer programlari kapatmanýzý öneririz. Böylece bilgisayarýnýzý yeniden baþlatmadan bazý sistem dosyalarý sorunsuz kurulabilir.$\r$\n$\r$\n$_CLICK"
!endif

!ifdef MUI_UNWELCOMEPAGE
  ${LangFileString} MUI_UNTEXT_WELCOME_INFO_TITLE "$(^NameDA) Programýný Kaldýrma Sihirbazýna Hoþ Geldiniz"
  ${LangFileString} MUI_UNTEXT_WELCOME_INFO_TEXT "Bu sihirbaz size $(^NameDA) programýnýn kadýrýlýmý boyunca rehberlik edecektir.$\r$\n$\r$\nKaldýrým iþlemeni baþlatmadan önce çalýþan diðer programlari kapatmanýzý öneririz. Böylece bilgisayarýnýzý yeniden baþlatmadan bazý sistem dosyalarý sorunsuz kaldýrýlabilir.$\r$\n$\r$\n$_CLICK"
!endif

!ifdef MUI_LICENSEPAGE
  ${LangFileString} MUI_TEXT_LICENSE_TITLE "Lisans Sözleþmesi"
  ${LangFileString} MUI_TEXT_LICENSE_SUBTITLE "Lütfen $(^NameDA) programýný kurmadan önce sözleþmeyi okuyunuz."
  ${LangFileString} MUI_INNERTEXT_LICENSE_BOTTOM "Sözleþme koþullarýný kabul ediyorsanýz, 'Kabul Ediyorum'a basýnýz. $(^NameDA) programýný kurmak için sözleþme koþullarýný kabul etmelisiniz."
  ${LangFileString} MUI_INNERTEXT_LICENSE_BOTTOM_CHECKBOX "Sözleþme koþullarýný kabul ediyorsanýz, aþaðýdaki onay kutusunu doldurunuz. $(^NameDA) programýný kurmak için sözleþme koþullarýný kabul etmelisiniz. $_CLICK"
  ${LangFileString} MUI_INNERTEXT_LICENSE_BOTTOM_RADIOBUTTONS "Sözleþme koþullarýný kabul ediyorsanýz, asagidaki onay düðmesini seçiniz. $(^NameDA) programýný kurmak için sözleþme koþullarýný kabul etmelisiniz. $_CLICK"
!endif

!ifdef MUI_UNLICENSEPAGE
  ${LangFileString} MUI_UNTEXT_LICENSE_TITLE "Lisans Sözleþmesi"
  ${LangFileString} MUI_UNTEXT_LICENSE_SUBTITLE "Lütfen $(^NameDA) programýný sisteminizden kaldýrmadan önce sözleþmeyi okuyunuz."
  ${LangFileString} MUI_UNINNERTEXT_LICENSE_BOTTOM "Sözleþme koþullarýný kabul ediyorsanýz, 'Kabul Ediyorum'a basýnýz. $(^NameDA) programýný sisteminizden kaldýrmak için sözleþme koþullarýný kabul etmelisiniz."
  ${LangFileString} MUI_UNINNERTEXT_LICENSE_BOTTOM_CHECKBOX "Sözleþme koþullarýný kabul ediyorsanýz, aþaðýdaki onay kutusunu doldurunuz. $(^NameDA) programýný sisteminizden kaldýrmak için sözleþme koþullarýný kabul etmelisiniz. $_CLICK"
  ${LangFileString} MUI_UNINNERTEXT_LICENSE_BOTTOM_RADIOBUTTONS "Sözleþme koþullarýný kabul ediyorsanýz, asagidaki onay düðmesini seçiniz. $(^NameDA) programýný sisteminizden kaldýrmak için sözleþme koþullarýný kabul etmelisiniz. $_CLICK"
!endif

!ifdef MUI_LICENSEPAGE | MUI_UNLICENSEPAGE
  ${LangFileString} MUI_INNERTEXT_LICENSE_TOP "Sözleþmenin geri kalanýný okumak için 'page down' tuþuna basabilirsiniz."
!endif

!ifdef MUI_COMPONENTSPAGE
  ${LangFileString} MUI_TEXT_COMPONENTS_TITLE "Bileþen seçimi"
  ${LangFileString} MUI_TEXT_COMPONENTS_SUBTITLE "Lütfen $(^NameDA) için kurmak istediginiz bileþenleri seçiniz."
!endif

!ifdef MUI_UNCOMPONENTSPAGE
  ${LangFileString} MUI_UNTEXT_COMPONENTS_TITLE "Bileþen Þeçimi"
  ${LangFileString} MUI_UNTEXT_COMPONENTS_SUBTITLE "Lütfen kaldýrmak istediðiniz $(^NameDA) program bileþenini seçiniz."
!endif

!ifdef MUI_COMPONENTSPAGE | MUI_UNCOMPONENTSPAGE
  ${LangFileString} MUI_INNERTEXT_COMPONENTS_DESCRIPTION_TITLE "Açýklama"
  !ifndef NSIS_CONFIG_COMPONENTPAGE_ALTERNATIVE
    ${LangFileString} MUI_INNERTEXT_COMPONENTS_DESCRIPTION_INFO "Bileþenlerin açýklamalarýný görmek için imleci bileþen üzerine götürün."
  !else
    ${LangFileString} MUI_INNERTEXT_COMPONENTS_DESCRIPTION_INFO "Bileþenlerin açýklamalarýný görmek için imleci bileþen üzerine götürün."
  !endif
!endif

!ifdef MUI_DIRECTORYPAGE
  ${LangFileString} MUI_TEXT_DIRECTORY_TITLE "Hedef dizini seçimi"
  ${LangFileString} MUI_TEXT_DIRECTORY_SUBTITLE "$(^NameDA) programýný kurmak istediðiniz dizini þeçiniz."
!endif

!ifdef MUI_UNDIRECTORYPAGE
  ${LangFileString} MUI_UNTEXT_DIRECTORY_TITLE "Kaldýrýlýcak Dizin Seçimi"
  ${LangFileString} MUI_UNTEXT_DIRECTORY_SUBTITLE "$(^NameDA) programýný kaldýrmak istediginiz dizini seçiniz."
!endif

!ifdef MUI_INSTFILESPAGE
  ${LangFileString} MUI_TEXT_INSTALLING_TITLE "Kuruluyor"
  ${LangFileString} MUI_TEXT_INSTALLING_SUBTITLE "Lütfen $(^NameDA) kurulurken bekleyiniz."
  ${LangFileString} MUI_TEXT_FINISH_TITLE "Kurulum Tamamlandý"
  ${LangFileString} MUI_TEXT_FINISH_SUBTITLE "Kurulum baþarýyla tamamlandý."
  ${LangFileString} MUI_TEXT_ABORT_TITLE "Kurulum Ýptal Edildi"
  ${LangFileString} MUI_TEXT_ABORT_SUBTITLE "Kurulum tam olarak tamamlanmadý."
!endif

!ifdef MUI_UNINSTFILESPAGE
  ${LangFileString} MUI_UNTEXT_UNINSTALLING_TITLE "Kaldýrýlýyor"
  ${LangFileString} MUI_UNTEXT_UNINSTALLING_SUBTITLE "Litfen $(^NameDA) programý sisteminizden kaldýrýlýrken bekleyiniz."
  ${LangFileString} MUI_UNTEXT_FINISH_TITLE "Kaldýrma Ýþlemi Tamamlandýr"
  ${LangFileString} MUI_UNTEXT_FINISH_SUBTITLE "Kaldýrma iþlemi baþarýyla tamamlandý."
  ${LangFileString} MUI_UNTEXT_ABORT_TITLE "Kaldýrma Ýþlemi Ýptal Edildi"
  ${LangFileString} MUI_UNTEXT_ABORT_SUBTITLE "Kaldýrma Ýþlemi tamamlanamadý."
!endif

!ifdef MUI_FINISHPAGE
  ${LangFileString} MUI_TEXT_FINISH_INFO_TITLE "$(^NameDA) Kurulum sihirbazý tamamlanýyor."
  ${LangFileString} MUI_TEXT_FINISH_INFO_TEXT "$(^NameDA)  bilgisayariniza yüklendi.$\r$\n$\r$\nLütfen 'Bitir'e basarak kurulumu sonlandýrýn."
  ${LangFileString} MUI_TEXT_FINISH_INFO_REBOOT "$(^NameDA) kurulumunun tamamlanmasý için bilgisayarýnýzý yeniden baþlatmanýz gerekiyor.Bilgisayarýnýzý yeniden baþlatmak istiyor musunuz?"
!endif

!ifdef MUI_UNFINISHPAGE
  ${LangFileString} MUI_UNTEXT_FINISH_INFO_TITLE "$(^NameDA) Programý Kaldýrma Sihirbazý Tamamlanýyor"
  ${LangFileString} MUI_UNTEXT_FINISH_INFO_TEXT "$(^NameDA) programý sisteminizden kaldýrýldý.$\r$\n$\r$\nSihirbazý kapatmak için 'bitir'e basýnýz."
  ${LangFileString} MUI_UNTEXT_FINISH_INFO_REBOOT "$(^NameDA) programýný kaldýrma iþleminin tamamlanmasý için bilgisayarýnýzýn yeniden baþlatýlmasý gerekiyor. Bilgisayarýnýzýn þimdi yeniden baþlatýlmasýný ister misiniz?"
!endif

!ifdef MUI_FINISHPAGE | MUI_UNFINISHPAGE
  ${LangFileString} MUI_TEXT_FINISH_REBOOTNOW "Yeniden baþlat"
  ${LangFileString} MUI_TEXT_FINISH_REBOOTLATER "Bilgisayarýmý daha sonra baþlatacaðým."
  ${LangFileString} MUI_TEXT_FINISH_RUN "$(^NameDA) programýný çalýþtýr"
  ${LangFileString} MUI_TEXT_FINISH_SHOWREADME "beni oku/readme dosyasýný &göster"
  ${LangFileString} MUI_BUTTONTEXT_FINISH "&Bitir"  
!endif

!ifdef MUI_STARTMENUPAGE
  ${LangFileString} MUI_TEXT_STARTMENU_TITLE "Baþlat Menüsü Klasör Seçimi"
  ${LangFileString} MUI_TEXT_STARTMENU_SUBTITLE "$(^NameDA) kýsayollarýnýn konulacagý baþlat menüsü klasörünü seçiniz."
  ${LangFileString} MUI_INNERTEXT_STARTMENU_TOP "Programýn kýsayollarýnýn konulacaðý baþlat menüsü klasörünü seçiniz. Farklý bir isim girerek yeni bir klasör yaratabilirsiniz."
  ${LangFileString} MUI_INNERTEXT_STARTMENU_CHECKBOX "Kýsayollarý oluþturmadan devam et"
!endif

!ifdef MUI_UNCONFIRMPAGE
  ${LangFileString} MUI_UNTEXT_CONFIRM_TITLE "$(^NameDA) Programýný Kaldýr"
  ${LangFileString} MUI_UNTEXT_CONFIRM_SUBTITLE "$(^NameDA) programýný sisteminizden kaldýrma."
!endif

!ifdef MUI_ABORTWARNING
  ${LangFileString} MUI_TEXT_ABORTWARNING "$(^Name) kurulumundan çýkmak istediðinize emin misiniz?"
!endif

!ifdef MUI_UNABORTWARNING
  ${LangFileString} MUI_UNTEXT_ABORTWARNING "$(^Name) Programi Kaldýrma iþleminden çýkmak istediðinize emin misiniz?"
!endif
