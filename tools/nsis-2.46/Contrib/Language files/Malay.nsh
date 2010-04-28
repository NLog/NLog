;Language: Malay (1086)
;By muhammadazwa@yahoo.com

!insertmacro LANGFILE "Malay" "Malay"

!ifdef MUI_WELCOMEPAGE
  ${LangFileString} MUI_TEXT_WELCOME_INFO_TITLE "Assalamualaikum, Selamat datang ke $(^NameDA) Setup Wizard"
  ${LangFileString} MUI_TEXT_WELCOME_INFO_TEXT "Setup Wizard akan membantu anda untuk memasukkan $(^NameDA).$\r$\n$\r$\nSila tutup program aplikasi yang lain sebelum Setup ini dimulakan. Ini supaya tiada proses reboot komputer diperlukan.$\r$\n$\r$\n$_CLICK"
!endif

!ifdef MUI_UNWELCOMEPAGE
  ${LangFileString} MUI_UNTEXT_WELCOME_INFO_TITLE "Selamat datang ke $(^NameDA) Uninstall Wizard"
  ${LangFileString} MUI_UNTEXT_WELCOME_INFO_TEXT "Uninstall Wizard akan membantu anda pada proses membuang $(^NameDA).$\r$\n$\r$\nSebelum membuang, pastikan dulu $(^NameDA) dimatikan.$\r$\n$\r$\n$_CLICK"
!endif

!ifdef MUI_LICENSEPAGE
  ${LangFileString} MUI_TEXT_LICENSE_TITLE "Perlesenan"
  ${LangFileString} MUI_TEXT_LICENSE_SUBTITLE "Sila baca teks lesen berikut sebelum memasukkan $(^NameDA)."
  ${LangFileString} MUI_INNERTEXT_LICENSE_BOTTOM "Jika anda bersetuju, klik Saya setuju untuk teruskan. Anda mesti setuju untuk sebelum aplikasi dapat dimasukkan $(^NameDA)."
  ${LangFileString} MUI_INNERTEXT_LICENSE_BOTTOM_CHECKBOX "Jika anda bersetuju dengan syarat-syarat lesen, sila tanda dicheckbox. Anda mesti setuju sebelum memasukkan $(^NameDA). $_CLICK"
  ${LangFileString} MUI_INNERTEXT_LICENSE_BOTTOM_RADIOBUTTONS "Jika anda terima semua yang ada di lesen, pilihlah salah satu item dibawah ini. Anda mesti setuju sebelum memasukkan $(^NameDA). $_CLICK"
!endif

!ifdef MUI_UNLICENSEPAGE
  ${LangFileString} MUI_UNTEXT_LICENSE_TITLE "Tentang Lesen"
  ${LangFileString} MUI_UNTEXT_LICENSE_SUBTITLE "Sila baca teks lesen sebelum membuang $(^NameDA)."
  ${LangFileString} MUI_UNINNERTEXT_LICENSE_BOTTOM "Jika anda menerima lesen, klik Saya setuju untuk teruskan. Anda mesti setuju untuk dapat membuang $(^NameDA)."
  ${LangFileString} MUI_UNINNERTEXT_LICENSE_BOTTOM_CHECKBOX "Jika anda menerima semua yang ada di lesen, beri tanda dicheckbox. Anda mesti setuju untuk dapat membuang $(^NameDA). $_CLICK"
  ${LangFileString} MUI_UNINNERTEXT_LICENSE_BOTTOM_RADIOBUTTONS "Jika anda menerima semua yang ada di lesen, pilihlah salah satu item dibawah ini. Anda mesti setuju untuk dapat membuang $(^NameDA). $_CLICK"
!endif

!ifdef MUI_LICENSEPAGE | MUI_UNLICENSEPAGE
  ${LangFileString} MUI_INNERTEXT_LICENSE_TOP "Tekan Page Down untuk melihat teks selebihnya."
!endif

!ifdef MUI_COMPONENTSPAGE
  ${LangFileString} MUI_TEXT_COMPONENTS_TITLE "Pilih Komponen"
  ${LangFileString} MUI_TEXT_COMPONENTS_SUBTITLE "Pilih fungsi-fungsi dari $(^NameDA) yang ingin dimasukkan."
!endif

!ifdef MUI_UNCOMPONENTSPAGE
  ${LangFileString} MUI_UNTEXT_COMPONENTS_TITLE "Pilih Komponen"
  ${LangFileString} MUI_UNTEXT_COMPONENTS_SUBTITLE "Pilih fungsi-fungsi $(^NameDA) yang ingin dibuang."
!endif

!ifdef MUI_COMPONENTSPAGE | MUI_UNCOMPONENTSPAGE
  ${LangFileString} MUI_INNERTEXT_COMPONENTS_DESCRIPTION_TITLE "Penerangan"
  !ifndef NSIS_CONFIG_COMPONENTPAGE_ALTERNATIVE
    ${LangFileString} MUI_INNERTEXT_COMPONENTS_DESCRIPTION_INFO "Alihkan tetikus ke komponen untuk mengetahui penerangannya."
  !else
    ${LangFileString} MUI_INNERTEXT_COMPONENTS_DESCRIPTION_INFO "Alihkan tetikus ke komponen untuk mengetahui penerangannya."
  !endif
!endif

!ifdef MUI_DIRECTORYPAGE
  ${LangFileString} MUI_TEXT_DIRECTORY_TITLE "Pilih Lokasi Kemasukan"
  ${LangFileString} MUI_TEXT_DIRECTORY_SUBTITLE "Pilih folder untuk memasukkan $(^NameDA)."
!endif

!ifdef MUI_UNDIRECTORYPAGE
  ${LangFileString} MUI_UNTEXT_DIRECTORY_TITLE "Pilih Lokasi Uninstall"
  ${LangFileString} MUI_UNTEXT_DIRECTORY_SUBTITLE "Pilih folder untuk meng-uninstall $(^NameDA)."
!endif

!ifdef MUI_INSTFILESPAGE
  ${LangFileString} MUI_TEXT_INSTALLING_TITLE "Pemasangan"
  ${LangFileString} MUI_TEXT_INSTALLING_SUBTITLE "Sila tunggu ketika $(^NameDA) sedang dimasukkan."
  ${LangFileString} MUI_TEXT_FINISH_TITLE "Proses Selesai"
  ${LangFileString} MUI_TEXT_FINISH_SUBTITLE "Setup sudah selesai."
  ${LangFileString} MUI_TEXT_ABORT_TITLE "Proses Dibatalkan"
  ${LangFileString} MUI_TEXT_ABORT_SUBTITLE "Setup terbatal."
!endif

!ifdef MUI_UNINSTFILESPAGE
  ${LangFileString} MUI_UNTEXT_UNINSTALLING_TITLE "Uninstall"
  ${LangFileString} MUI_UNTEXT_UNINSTALLING_SUBTITLE "Sila tunggu ketika $(^NameDA) sedang di-buang."
  ${LangFileString} MUI_UNTEXT_FINISH_TITLE "Proses Uninstall Selesai"
  ${LangFileString} MUI_UNTEXT_FINISH_SUBTITLE "Uninstall sudah selesai."
  ${LangFileString} MUI_UNTEXT_ABORT_TITLE "Proses Uninstall Dibatalkan"
  ${LangFileString} MUI_UNTEXT_ABORT_SUBTITLE "Uninstall belum selesai secara sempurna."
!endif

!ifdef MUI_FINISHPAGE
  ${LangFileString} MUI_TEXT_FINISH_INFO_TITLE "Menyelesaikan $(^NameDA) Setup Wizard"
  ${LangFileString} MUI_TEXT_FINISH_INFO_TEXT "$(^NameDA) sudah dimasukkan di komputer anda.$\r$\n$\r$\nKlik Selesai untuk menutup Setup Wizard."
  ${LangFileString} MUI_TEXT_FINISH_INFO_REBOOT "Komputer anda harus direboot untuk menyelesaikan proses memasukkan $(^NameDA). Apakah anda hendak reboot sekarang juga?"
!endif

!ifdef MUI_UNFINISHPAGE
  ${LangFileString} MUI_UNTEXT_FINISH_INFO_TITLE "Menyelesaikan $(^NameDA) Uninstall Wizard"
  ${LangFileString} MUI_UNTEXT_FINISH_INFO_TEXT "$(^NameDA) sudah dibuang dari komputer anda.$\r$\n$\r$\nKlik Selesai untuk menutup Setup Wizard."
  ${LangFileString} MUI_UNTEXT_FINISH_INFO_REBOOT "Komputer anda harus di reboot untuk menyelesaikan proses membuang $(^NameDA). Reboot sekarang?"
!endif

!ifdef MUI_FINISHPAGE | MUI_UNFINISHPAGE
  ${LangFileString} MUI_TEXT_FINISH_REBOOTNOW "Reboot sekarang"
  ${LangFileString} MUI_TEXT_FINISH_REBOOTLATER "Reboot nanti"
  ${LangFileString} MUI_TEXT_FINISH_RUN "&Jalankan $(^NameDA)"
  ${LangFileString} MUI_TEXT_FINISH_SHOWREADME "&Buka fail Readme"
  ${LangFileString} MUI_BUTTONTEXT_FINISH "&Selesai"  
!endif

!ifdef MUI_STARTMENUPAGE
  ${LangFileString} MUI_TEXT_STARTMENU_TITLE "Pilih Folder Start Menu"
  ${LangFileString} MUI_TEXT_STARTMENU_SUBTITLE "Pilih folder Start Menu untuk meletakkan pintasan $(^NameDA)."
  ${LangFileString} MUI_INNERTEXT_STARTMENU_TOP "Pilih folder Start Menu untuk perletakkan pintasan aplikasi ini. Boleh cipta nama folder anda sendiri."
  ${LangFileString} MUI_INNERTEXT_STARTMENU_CHECKBOX "Tidak perlu pintasan"
!endif

!ifdef MUI_UNCONFIRMPAGE
  ${LangFileString} MUI_UNTEXT_CONFIRM_TITLE "Buang $(^NameDA)"
  ${LangFileString} MUI_UNTEXT_CONFIRM_SUBTITLE "Padam $(^NameDA) dari komputer anda."
!endif

!ifdef MUI_ABORTWARNING
  ${LangFileString} MUI_TEXT_ABORTWARNING "Adakan anda yakin ingin membatalkan Setup $(^Name)?"
!endif

!ifdef MUI_UNABORTWARNING
  ${LangFileString} MUI_UNTEXT_ABORTWARNING "Adakan anda yakin ingin membatalkan proses buang $(^Name)?"
!endif
