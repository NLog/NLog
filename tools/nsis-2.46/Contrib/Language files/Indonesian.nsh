;Language: Indonesian (1057)
;By Ariel825010106@yahoo.com modified by was.uthm@gmail.com in April 2009

!insertmacro LANGFILE "Indonesian" "Indonesian"

!ifdef MUI_WELCOMEPAGE
  ${LangFileString} MUI_TEXT_WELCOME_INFO_TITLE "Selamat datang di program instalasi $(^NameDA)"
  ${LangFileString} MUI_TEXT_WELCOME_INFO_TEXT "Program ini akan membantu anda dalam proses instalasi $(^NameDA).$\r$\n$\r$\nAnda sangat disarankan untuk menutup program lainnya sebelum memulai proses instalasi. Hal ini diperlukan agar berkas yang terkait dapat diperbarui tanpa harus booting ulang komputer anda.$\r$\n$\r$\n$_CLICK"
!endif

!ifdef MUI_UNWELCOMEPAGE
  ${LangFileString} MUI_UNTEXT_WELCOME_INFO_TITLE "Selamat datang di program penghapusan $(^NameDA)"
  ${LangFileString} MUI_UNTEXT_WELCOME_INFO_TEXT "Program ini akan membantu anda pada proses penghapusan $(^NameDA).$\r$\n$\r$\nSebelum memulai proses penghapusan, pastikan dulu $(^NameDA) tidak sedang digunakan.$\r$\n$\r$\n$_CLICK"
!endif

!ifdef MUI_LICENSEPAGE
  ${LangFileString} MUI_TEXT_LICENSE_TITLE "Perihal Lisensi"
  ${LangFileString} MUI_TEXT_LICENSE_SUBTITLE "Silahkan membaca perihal lisensi sebelum memulai proses instalasi $(^NameDA)."
  ${LangFileString} MUI_INNERTEXT_LICENSE_BOTTOM "Jika anda setuju dan menerima semua pernyataan, tekan tombol Saya Setuju untuk melanjutkan. Anda harus setuju untuk memulai instalasi $(^NameDA)."
  ${LangFileString} MUI_INNERTEXT_LICENSE_BOTTOM_CHECKBOX "Jika anda setuju dan menerima semua pernyatan, beri tanda centang. Anda harus setuju untuk memulai instalasi $(^NameDA). $_CLICK"
  ${LangFileString} MUI_INNERTEXT_LICENSE_BOTTOM_RADIOBUTTONS "Jika anda setuju dan menerima semua pernyataan, pilihlah salah satu item dibawah ini. Anda harus setuju untuk memulai instalasi $(^NameDA). $_CLICK"
!endif

!ifdef MUI_UNLICENSEPAGE
  ${LangFileString} MUI_UNTEXT_LICENSE_TITLE "Perihal Lisensi"
  ${LangFileString} MUI_UNTEXT_LICENSE_SUBTITLE "Silahkan membaca lisensi berikut sebelum melakukan penghapusan $(^NameDA)."
  ${LangFileString} MUI_UNINNERTEXT_LICENSE_BOTTOM "Jika anda setuju dan menerima semua pernyataan, tekan tombol Saya Setuju untuk melanjutkan. Anda harus setuju untuk memulai proses penghapusan $(^NameDA)."
  ${LangFileString} MUI_UNINNERTEXT_LICENSE_BOTTOM_CHECKBOX "Jika anda setuju dan menerima semua pernyataan, beri tanda centang. Anda harus setuju untuk memulai proses penghapusan $(^NameDA). $_CLICK"
  ${LangFileString} MUI_UNINNERTEXT_LICENSE_BOTTOM_RADIOBUTTONS "Jika anda setuju dan menerima semua pernyataan, pilihlah salah satu item dibawah ini. Anda harus setuju untuk memulai proses penghapusan $(^NameDA). $_CLICK"
!endif

!ifdef MUI_LICENSEPAGE | MUI_UNLICENSEPAGE
  ${LangFileString} MUI_INNERTEXT_LICENSE_TOP "Tekan tombol Page Down untuk melihat pernyataan berikutnya."
!endif

!ifdef MUI_COMPONENTSPAGE
  ${LangFileString} MUI_TEXT_COMPONENTS_TITLE "Pilih Komponen"
  ${LangFileString} MUI_TEXT_COMPONENTS_SUBTITLE "Pilih komponen fitur tambahan dari $(^NameDA) yang ingin di instal."
!endif

!ifdef MUI_UNCOMPONENTSPAGE
  ${LangFileString} MUI_UNTEXT_COMPONENTS_TITLE "Pilih Komponen"
  ${LangFileString} MUI_UNTEXT_COMPONENTS_SUBTITLE "Pilih komponen fitur tambahan dari $(^NameDA) yang ingin dihapus."
!endif

!ifdef MUI_COMPONENTSPAGE | MUI_UNCOMPONENTSPAGE
  ${LangFileString} MUI_INNERTEXT_COMPONENTS_DESCRIPTION_TITLE "Deskripsi"
  !ifndef NSIS_CONFIG_COMPONENTPAGE_ALTERNATIVE
    ${LangFileString} MUI_INNERTEXT_COMPONENTS_DESCRIPTION_INFO "Tunjuk ke salah satu komponen untuk melihat deskripsi tentang komponen itu."
  !else
    ${LangFileString} MUI_INNERTEXT_COMPONENTS_DESCRIPTION_INFO "Tunjuk ke salah satu komponen untuk melihat deskripsi tentang komponen itu."
  !endif
!endif

!ifdef MUI_DIRECTORYPAGE
  ${LangFileString} MUI_TEXT_DIRECTORY_TITLE "Pilih Lokasi Instalasi"
  ${LangFileString} MUI_TEXT_DIRECTORY_SUBTITLE "Pilih lokasi untuk instalasi program $(^NameDA)."
!endif

!ifdef MUI_UNDIRECTORYPAGE
  ${LangFileString} MUI_UNTEXT_DIRECTORY_TITLE "Pilih Lokasi berkas yang akan dihapus"
  ${LangFileString} MUI_UNTEXT_DIRECTORY_SUBTITLE "Pilih lokasi instalasi program $(^NameDA) yang akan dihapus."
!endif

!ifdef MUI_INSTFILESPAGE
  ${LangFileString} MUI_TEXT_INSTALLING_TITLE "Proses instalasi "
  ${LangFileString} MUI_TEXT_INSTALLING_SUBTITLE "Mohon tunggu sejenak, instalasi program $(^NameDA) sedang berlangsung."
  ${LangFileString} MUI_TEXT_FINISH_TITLE "Instalasi Selesai"
  ${LangFileString} MUI_TEXT_FINISH_SUBTITLE "Seluruh proses instalasi sudah paripurna."
  ${LangFileString} MUI_TEXT_ABORT_TITLE "Instalasi Dibatalkan"
  ${LangFileString} MUI_TEXT_ABORT_SUBTITLE "Proses instalasi tidak selesai dengan sempurna."
!endif

!ifdef MUI_UNINSTFILESPAGE
  ${LangFileString} MUI_UNTEXT_UNINSTALLING_TITLE "Proses penghapusan"
  ${LangFileString} MUI_UNTEXT_UNINSTALLING_SUBTITLE "Mohon tunggu sejenak, penghapusan program $(^NameDA) sedang berlangsung."
  ${LangFileString} MUI_UNTEXT_FINISH_TITLE "Proses Penghapusan Selesai"
  ${LangFileString} MUI_UNTEXT_FINISH_SUBTITLE "Seluruh proses penghapusan sudah paripurna."
  ${LangFileString} MUI_UNTEXT_ABORT_TITLE "Penghapusan Dibatalkan"
  ${LangFileString} MUI_UNTEXT_ABORT_SUBTITLE "Proses penghapusa tidak selesai dengan sempurna."
!endif

!ifdef MUI_FINISHPAGE
  ${LangFileString} MUI_TEXT_FINISH_INFO_TITLE "Menutup Instalasi Program $(^NameDA)"
  ${LangFileString} MUI_TEXT_FINISH_INFO_TEXT "$(^NameDA) sudah di instal di komputer anda.$\r$\n$\r$\nTekan tombol Selesai untuk menutup program."
  ${LangFileString} MUI_TEXT_FINISH_INFO_REBOOT "Komputer anda memerlukan booting ulang untuk menyempurnakan proses instalasi $(^NameDA). Apakah anda akan melakukan booting ulang sekarang juga?"
!endif

!ifdef MUI_UNFINISHPAGE
  ${LangFileString} MUI_UNTEXT_FINISH_INFO_TITLE "Menutup program penghapusan $(^NameDA)"
  ${LangFileString} MUI_UNTEXT_FINISH_INFO_TEXT "$(^NameDA) sudah dihapus dari komputer anda.$\r$\n$\r$\nTekan tombol Selesai untuk menutup."
  ${LangFileString} MUI_UNTEXT_FINISH_INFO_REBOOT "Komputer anda memerlukan booting untuk menyempurnakan proses penghapusan $(^NameDA). Apakah anda akan melakukan booting ulang sekarang juga?"
!endif

!ifdef MUI_FINISHPAGE | MUI_UNFINISHPAGE
  ${LangFileString} MUI_TEXT_FINISH_REBOOTNOW "Booting ulang sekarang"
  ${LangFileString} MUI_TEXT_FINISH_REBOOTLATER "Booting ulang nanti"
  ${LangFileString} MUI_TEXT_FINISH_RUN "&Jalankan $(^NameDA)"
  ${LangFileString} MUI_TEXT_FINISH_SHOWREADME "&Buka berkas Readme"
  ${LangFileString} MUI_BUTTONTEXT_FINISH "&Selesai"  
!endif

!ifdef MUI_STARTMENUPAGE
  ${LangFileString} MUI_TEXT_STARTMENU_TITLE "Pilih lokasi dari Menu Start"
  ${LangFileString} MUI_TEXT_STARTMENU_SUBTITLE "Pilih lokasi dari Menu Start untuk meletakkan shortcut $(^NameDA)."
  ${LangFileString} MUI_INNERTEXT_STARTMENU_TOP "Pilih lokasi dari Menu Start untuk meletakkan shortcut program ini. Anda bisa juga membuat lokasi baru dengan cara menulis nama lokasi yang dikehendaki."
  ${LangFileString} MUI_INNERTEXT_STARTMENU_CHECKBOX "Tidak perlu membuat shortcut"
!endif

!ifdef MUI_UNCONFIRMPAGE
  ${LangFileString} MUI_UNTEXT_CONFIRM_TITLE "Penghapusan $(^NameDA)"
  ${LangFileString} MUI_UNTEXT_CONFIRM_SUBTITLE "Menghapus $(^NameDA) dari komputer anda."
!endif

!ifdef MUI_ABORTWARNING
  ${LangFileString} MUI_TEXT_ABORTWARNING "Apakah anda yakin ingin menghentikan proses instalasi $(^Name)?"
!endif

!ifdef MUI_UNABORTWARNING
  ${LangFileString} MUI_UNTEXT_ABORTWARNING "Apakah anda yakin ingin menghentikan proses penghapusan $(^Name)?"
!endif

!ifdef MULTIUSER_INSTALLMODEPAGE
  ${LangFileString} MULTIUSER_TEXT_INSTALLMODE_TITLE "Pilihan Pemakai"
  ${LangFileString} MULTIUSER_TEXT_INSTALLMODE_SUBTITLE "Pilihlah pemakai komputer yang akan menggunakan program $(^NameDA)."
  ${LangFileString} MULTIUSER_INNERTEXT_INSTALLMODE_TOP "Apakah anda akan melakukan instalasi $(^NameDA) untuk anda sendiri atau untuk semua pemakai komputer ini. $(^ClickNext)"
  ${LangFileString} MULTIUSER_INNERTEXT_INSTALLMODE_ALLUSERS "Instalasi untuk semua pemakai komputer ini"
  ${LangFileString} MULTIUSER_INNERTEXT_INSTALLMODE_CURRENTUSER "Instalasi hanya untuk saya sendiri"
!endif
