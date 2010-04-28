!ifndef COLORS_NSH
!define COLORS_NSH

!verbose push
!verbose 3

# Squad
# Rob Segal
# Joel
# Yathosho


# Predefined HTML Hex colors
!define WHITE "FFFFFF"
!define BLACK "000000"
!define YELLOW "FFFF00"
!define RED "FF0000"
!define GREEN "00FF00"
!define BLUE "0000FF"
!define MAGENTA "FF00FF"
!define CYAN "00FFFF"

# Function to convert red , green and blue integer values to HTML Hex format
!define RGB '!insertmacro rgb2hex'

# Function to convert red, green and blue integer values to Hexadecimal (0xRRGGBB) format
!define HEX '!insertmacro rgb2hex2'

# Function to get the r value from a RGB number
!define GetRvalue '!insertmacro redvalue'

# Function to get the g value from a RGB number
!define GetGvalue '!insertmacro greenvalue'

# Function to get the b value from a RGB number
!define GetBvalue '!insertmacro bluevalue'

# Function to get the r value from a Hex number
!define GetRvalueX '!insertmacro bluevalue'

# Function to get the g value from a Hex number
!define GetGvalueX '!insertmacro greenvalue'

# Function to get the r value from a HEX number
!define GetBvalueX '!insertmacro redvalue'

!macro rgb2hex output R G B
IntFmt "${output}" "%02X" "${R}"
IntFmt "${output}" "${output}%02X" "${G}"
IntFmt "${output}" "${output}%02X" "${B}"
!macroend

!macro rgb2hex2 output R G B
IntFmt "${output}" "%02X" "${B}"
IntFmt "${output}" "${output}%02X" "${G}"
IntFmt "${output}" "${output}%02X" "${R}"
!macroend

!macro redvalue output hexval
StrCpy ${output} ${hexval} 2 0
IntFmt "${output}" "%02i" "0x${output}"
!macroend

!macro greenvalue output hexval
StrCpy ${output} ${hexval} 2 2
IntFmt "${output}" "%02i" "0x${output}"
!macroend

!macro bluevalue output hexval
StrCpy ${output} ${hexval} 2 4
IntFmt "${output}" "%02i" "0x${output}"
!macroend

!verbose pop
!endif