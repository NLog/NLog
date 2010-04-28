;Language: Japanese (1041)
;By Dnanako
;Translation updated by Takahiro Yoshimura <takahiro_y@monolithworks.co.jp>

!insertmacro LANGFILE "Japanese" "Japanese"

!ifdef MUI_WELCOMEPAGE
  ${LangFileString} MUI_TEXT_WELCOME_INFO_TITLE "$(^NameDA) セットアップ ウィザードへようこそ"
  ${LangFileString} MUI_TEXT_WELCOME_INFO_TEXT "このウィザードは、$(^NameDA)のインストールをガイドしていきます。$\r$\n$\r$\nセットアップを開始する前に、他のすべてのアプリケーションを終了することを推奨します。これによってセットアップがコンピュータを再起動せずに、システム ファイルを更新することが出来るようになります。$\r$\n$\r$\n$_CLICK"
!endif

!ifdef MUI_UNWELCOMEPAGE
  ${LangFileString} MUI_UNTEXT_WELCOME_INFO_TITLE "$(^NameDA) アンインストール ウィザードへようこそ"
  ${LangFileString} MUI_UNTEXT_WELCOME_INFO_TEXT "このウィザードは、$(^NameDA)のアンインストールをガイドしていきます。$\r$\n$\r$\nアンインストールを開始する前に、$(^NameDA)が起動していないことを確認して下さい。$\r$\n$\r$\n$_CLICK"
!endif

!ifdef MUI_LICENSEPAGE
  ${LangFileString} MUI_TEXT_LICENSE_TITLE "ライセンス契約書"
  ${LangFileString} MUI_TEXT_LICENSE_SUBTITLE "$(^NameDA)をインストールする前に、ライセンス条件を確認してください。"
  ${LangFileString} MUI_INNERTEXT_LICENSE_BOTTOM "契約書のすべての条件に同意するならば、[同意する] を選んでインストールを続けてください。$(^NameDA) をインストールするには、契約書に同意する必要があります。"
  ${LangFileString} MUI_INNERTEXT_LICENSE_BOTTOM_CHECKBOX "契約書のすべての条件に同意するならば、下のチェックボックスをクリックしてください。$(^NameDA) をインストールするには、契約書に同意する必要があります。 $_CLICK"
  ${LangFileString} MUI_INNERTEXT_LICENSE_BOTTOM_RADIOBUTTONS "契約書のすべての条件に同意するならば、下に表示されているオプションのうち、最初のものを選んで下さい。$(^NameDA) をインストールするには、契約書に同意する必要があります。 $_CLICK"
!endif

!ifdef MUI_UNLICENSEPAGE
  ${LangFileString} MUI_UNTEXT_LICENSE_TITLE "ライセンス契約書"
  ${LangFileString} MUI_UNTEXT_LICENSE_SUBTITLE "$(^NameDA)をアンインストールする前に、ライセンス条件を確認してください。"
  ${LangFileString} MUI_UNINNERTEXT_LICENSE_BOTTOM "契約書のすべての条件に同意するならば、[同意する] を選んでアンインストールを続けてください。$(^NameDA) をアンインストールするには、契約書に同意する必要があります。"
  ${LangFileString} MUI_UNINNERTEXT_LICENSE_BOTTOM_CHECKBOX "契約書のすべての条件に同意するならば、下のチェックボックスをクリックしてください。$(^NameDA) をアンインストールするには、契約書に同意する必要があります。 $_CLICK"
  ${LangFileString} MUI_UNINNERTEXT_LICENSE_BOTTOM_RADIOBUTTONS "契約書のすべての条件に同意するならば、下に表示されているオプションのうち、最初のものを選んで下さい。$(^NameDA) をアンインストールするには、契約書に同意する必要があります。 $_CLICK"
!endif

!ifdef MUI_LICENSEPAGE | MUI_UNLICENSEPAGE
  ${LangFileString} MUI_INNERTEXT_LICENSE_TOP "[Page Down]を押して契約書をすべてお読みください。"
!endif

!ifdef MUI_COMPONENTSPAGE
  ${LangFileString} MUI_TEXT_COMPONENTS_TITLE "コンポーネントを選んでください。"
  ${LangFileString} MUI_TEXT_COMPONENTS_SUBTITLE "$(^NameDA)のインストール オプションを選んでください。"
!endif

!ifdef MUI_UNCOMPONENTSPAGE
  ${LangFileString} MUI_UNTEXT_COMPONENTS_TITLE "コンポーネントを選んでください。"
  ${LangFileString} MUI_UNTEXT_COMPONENTS_SUBTITLE "$(^NameDA)のアンインストール オプションを選んでください。"
!endif

!ifdef MUI_COMPONENTSPAGE | MUI_UNCOMPONENTSPAGE
  ${LangFileString} MUI_INNERTEXT_COMPONENTS_DESCRIPTION_TITLE "説明"
  !ifndef NSIS_CONFIG_COMPONENTPAGE_ALTERNATIVE
    ${LangFileString} MUI_INNERTEXT_COMPONENTS_DESCRIPTION_INFO "コンポーネントの上にマウス カーソルを移動すると、ここに説明が表示されます。"
  !else
    ${LangFileString} MUI_INNERTEXT_COMPONENTS_DESCRIPTION_INFO "コンポーネントの上にマウス カーソルを移動すると、ここに説明が表示されます。"
  !endif
!endif

!ifdef MUI_DIRECTORYPAGE
  ${LangFileString} MUI_TEXT_DIRECTORY_TITLE "インストール先を選んでください。"
  ${LangFileString} MUI_TEXT_DIRECTORY_SUBTITLE "$(^NameDA)をインストールするフォルダを選んでください。"
!endif

!ifdef MUI_UNDIRECTORYPAGE
  ${LangFileString} MUI_UNTEXT_DIRECTORY_TITLE "アンインストール元を選んでください。"
  ${LangFileString} MUI_UNTEXT_DIRECTORY_SUBTITLE "$(^NameDA)をアンインストールするフォルダを選んでください。"
!endif

!ifdef MUI_INSTFILESPAGE
  ${LangFileString} MUI_TEXT_INSTALLING_TITLE "インストール"
  ${LangFileString} MUI_TEXT_INSTALLING_SUBTITLE "$(^NameDA)をインストールしています。しばらくお待ちください。"
  ${LangFileString} MUI_TEXT_FINISH_TITLE "インストールの完了"
  ${LangFileString} MUI_TEXT_FINISH_SUBTITLE "インストールに成功しました。"
  ${LangFileString} MUI_TEXT_ABORT_TITLE "インストールの中止"
  ${LangFileString} MUI_TEXT_ABORT_SUBTITLE "セットアップは正常に完了されませんでした。"
!endif

!ifdef MUI_UNINSTFILESPAGE
  ${LangFileString} MUI_UNTEXT_UNINSTALLING_TITLE "アンインストール"
  ${LangFileString} MUI_UNTEXT_UNINSTALLING_SUBTITLE "$(^NameDA)をアンインストールしています。しばらくお待ちください。"
  ${LangFileString} MUI_UNTEXT_FINISH_TITLE "アンインストールの完了"
  ${LangFileString} MUI_UNTEXT_FINISH_SUBTITLE "アンインストールに成功しました。"
  ${LangFileString} MUI_UNTEXT_ABORT_TITLE "アンインストールの中止"
  ${LangFileString} MUI_UNTEXT_ABORT_SUBTITLE "アンインストールは正常に完了されませんでした。"
!endif

!ifdef MUI_FINISHPAGE
  ${LangFileString} MUI_TEXT_FINISH_INFO_TITLE "$(^NameDA) セットアップ ウィザードは完了しました。"
  ${LangFileString} MUI_TEXT_FINISH_INFO_TEXT "$(^NameDA)は、このコンピュータにインストールされました。$\r$\n$\r$\nウィザードを閉じるには [完了] を押してください。"
  ${LangFileString} MUI_TEXT_FINISH_INFO_REBOOT "$(^NameDA) のインストールを完了するには、このコンピュータを再起動する必要があります。今すぐ再起動しますか？"
!endif

!ifdef MUI_UNFINISHPAGE
  ${LangFileString} MUI_UNTEXT_FINISH_INFO_TITLE "$(^NameDA) アンインストール ウィザードは完了しました。"
  ${LangFileString} MUI_UNTEXT_FINISH_INFO_TEXT "$(^NameDA)は、このコンピュータからアンインストールされました。$\r$\n$\r$\nウィザードを閉じるには [完了] を押してください。"
  ${LangFileString} MUI_UNTEXT_FINISH_INFO_REBOOT "$(^NameDA) のアンインストールを完了するには、このコンピュータを再起動する必要があります。今すぐ再起動しますか？"
!endif

!ifdef MUI_FINISHPAGE | MUI_UNFINISHPAGE
  ${LangFileString} MUI_TEXT_FINISH_REBOOTNOW "今すぐ再起動する"
  ${LangFileString} MUI_TEXT_FINISH_REBOOTLATER "後で手動で再起動する"
  ${LangFileString} MUI_TEXT_FINISH_RUN "$(^NameDA)を実行(&R)"
  ${LangFileString} MUI_TEXT_FINISH_SHOWREADME "Readme を表示する(&S)"
  ${LangFileString} MUI_BUTTONTEXT_FINISH "完了(&F)"  
!endif

!ifdef MUI_STARTMENUPAGE
  ${LangFileString} MUI_TEXT_STARTMENU_TITLE "スタートメニュー フォルダを選んでください。"
  ${LangFileString} MUI_TEXT_STARTMENU_SUBTITLE "$(^NameDA)のショートカットを作成するスタートメニュー フォルダを選んで下さい。"
  ${LangFileString} MUI_INNERTEXT_STARTMENU_TOP "このプログラムのショートカットを作成したいスタートメニュー フォルダを選択してください。また、作成する新しいフォルダに名前をつけることもできます。"
  ${LangFileString} MUI_INNERTEXT_STARTMENU_CHECKBOX "ショートカットを作成しない"
!endif

!ifdef MUI_UNCONFIRMPAGE
  ${LangFileString} MUI_UNTEXT_CONFIRM_TITLE "$(^NameDA)のアンインストール"
  ${LangFileString} MUI_UNTEXT_CONFIRM_SUBTITLE "$(^NameDA)をこのコンピュータから削除します。"
!endif

!ifdef MUI_ABORTWARNING
  ${LangFileString} MUI_TEXT_ABORTWARNING "$(^Name) セットアップを中止しますか？"
!endif

!ifdef MUI_UNABORTWARNING
  ${LangFileString} MUI_UNTEXT_ABORTWARNING "$(^Name) アンインストールを中止しますか？"
!endif
