; This is just an example of Math plugin
; 
; (c) brainsucker, 2002
; (r) BSForce

Name "Math Plugin Example"
OutFile "math.exe"
ShowInstDetails show
XPStyle on

Section "ThisNameIsIgnoredSoWhyBother?"
    Math::Script 'SaR(s,fa,ra, i,f,r,e,p) (i=0;#{i<l(fa),e=l(f=fa[i]);r=ra[i];p=0;#{p<l(s),#[s[p,p+e-1]==f,s=(s[,p-1])+r+(s[p+e,]);p+=l(r), p++]}; i++}; s);'
    Math::Script "TQ(s) (s = s(NS); #[s[0]=='$\"',s=s[1,]]; #[s[-1]=='$\"',s=s[,-2]]; NS = s)"
    Math::Script "P(s,e, p,i) (p=-1;i=0; #{(i<l(s))&&(p<0), #[s[i,i+l(e)-1]==e, p=i]; i++}; p)"
    Math::Script "DL(s) (s=s(NS); p=P(s,'\r\n'); #[p>=0, (NS=s[p+4,]; NS=#[p>0,s[,p-1],'']), (NS='';NS=s)])"

    Math::Script "a = 'Hello \r\n World \r\n!!!'; a = SaR(a,{'\r','\n'},{'$\r','$\n'}); R0 = a"
    Math::Script "NS = '$\"In quotes$\"'; TQ(); R1=NS; R3=P(s(R1),'qu')"
    Math::Script "NS = 'No quotes'; TQ(); R2=NS"
    Math::Script "NS='123\r\n456\r\n789'; DL(); R4=NS; DL(); R5=NS; DL(); R6=NS; R7=NS"


    DetailPrint "'$R0'"
    DetailPrint "'$R1'"
    DetailPrint "'$R2'"
    DetailPrint "'$R3'"
    DetailPrint "'$R4'"
    DetailPrint "'$R5'"
    DetailPrint "'$R6'"
    DetailPrint "'$R7'"
SectionEnd 

; eof
