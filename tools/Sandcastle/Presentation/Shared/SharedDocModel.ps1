
# Shared Sandcastle build script overrides.

function CreateOutputTemplate {
	WriteInfo "Creating output template."
    MakePath $TempDir\Output\html
    MakePath $TempDir\Output\Media
    copy-item -r $DxRoot\Presentation\$Style\icons $TempDir\Output
    copy-item -r $DxRoot\Presentation\$Style\scripts $TempDir\Output
    copy-item -r $DxRoot\Presentation\$Style\styles $TempDir\Output
}

function CreateChmTemplate {
	WriteInfo "Creating CHM template."
    MakePath $TempDir\Chm\html
    copy-item -r $TempDir\Output\media $TempDir\Chm
    copy-item -r $TempDir\Output\icons $TempDir\Chm
    copy-item -r $TempDir\Output\scripts $TempDir\Chm
    copy-item -r $TempDir\Output\styles $TempDir\Chm

    MakePath $TempDir\Intellisense
}

function CreateHxsTemplate {
	WriteInfo "Creating HxS template."
	$s = "$DxRoot\Presentation\Shared\HxsTemplate"
	$d = "$TempDir\Output\$Name"
	copy-item $s\template.HxF "$($d).HxF"
	copy-item $s\template_A.HxK "$($d)_A.HxK"
	copy-item $s\template_B.HxK "$($d)_B.HxK"
	copy-item $s\template_F.HxK "$($d)_F.HxK"
	copy-item $s\template_K.HxK "$($d)_K.HxK"
	copy-item $s\template_N.HxK "$($d)_N.HxK"
	copy-item $s\template_S.HxK "$($d)_S.HxK"
}

function CreateWebsiteTemplate {
	WriteInfo "Creating website template."
	MakePath $WebOutputDir
	MakePath $WebOutputDir\api
    copy-item -r $DxRoot\Presentation\$Style\icons $WebOutputDir
    copy-item -r $DxRoot\Presentation\$Style\styles $WebOutputDir
    copy-item -r -force $WebTemplate\* $WebOutputDir
}