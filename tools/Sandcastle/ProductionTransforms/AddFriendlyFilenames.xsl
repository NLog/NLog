<?xml version="1.0"?>
<xsl:stylesheet
	xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
	xmlns:msxsl="urn:schemas-microsoft-com:xslt"
	xmlns:ddue="urn:ddue-extensions"
	version="1.1">

	<msxsl:script language="C#" implements-prefix="ddue">
		<msxsl:using namespace="System" />
		<![CDATA[
			public static string getFileName(string id) {
        string fileName = id.Replace(':', '_').Replace('<', '_').Replace('>', '_');

			  if (fileName.IndexOf(".#ctor") != -1 && fileName.IndexOf("Overload") == -1)
			  {
				  fileName = "C_" + fileName.Substring(2);
				  fileName = fileName.Replace(".#ctor", ".ctor");
			  }
        else if (fileName.IndexOf(".#ctor") != -1 && fileName.IndexOf("Overload") != -1)
			  {
				  fileName = fileName.Replace("Overload", "O_T");
				  fileName = fileName.Replace(".#ctor", ".ctor");
			  }
			  else if (fileName.IndexOf(".#cctor") != -1 && fileName.IndexOf("Overload") == -1)
			  {
				  fileName = "C_" + fileName.Substring(2);
				  fileName = fileName.Replace(".#cctor", ".cctor");
			  }
        else if (fileName.IndexOf(".#cctor") != -1 && fileName.IndexOf("Overload") != -1)
			  {
				  fileName = fileName.Replace("Overload", "O_T");
				  fileName = fileName.Replace(".#cctor", ".cctor");
			  }
        else if (fileName.IndexOf("Overload") != -1)
        {
          fileName = fileName.Replace("Overload", "O_T");
        }

			  fileName = fileName.Replace('.', '_').Replace('#', '_');
			
			  int paramStart = fileName.IndexOf('(');
			  if(paramStart != -1)
			  {
				  fileName = fileName.Substring(0, paramStart) + GenerateParametersCode(id.Substring(paramStart));
			  }

			  return fileName;
			}
      
      private static string GenerateParametersCode(string parameterSection)
		  {
			  // TODO: figure out a consistent algorithm that works regardless of runtime version
			  int code = parameterSection.GetHashCode();
			
			  int parameterCount = 1;
			
			  for(int count = 0; count < parameterSection.Length; count += 1)
			  {
				  int c = (int) parameterSection[count];

				  if(c == ',')
					  ++parameterCount;
			  }

			  // format as (# of parameters)_(semi-unique hex code)
			  return string.Format("_{1}_{0:x8}", code, parameterCount);
		}
		]]>
	</msxsl:script>

	<xsl:output indent="yes" encoding="UTF-8" />
	
	<xsl:template match="/">
		<reflection>
			<xsl:apply-templates select="/*/assemblies" />
			<xsl:apply-templates select="/*/apis" />
		</reflection>
	</xsl:template>

	<xsl:template match="assemblies">
		<xsl:copy-of select="." />
	</xsl:template>

	<xsl:template match="apis">
		<apis>
			<xsl:apply-templates select="api" />
		</apis>
	</xsl:template>

	<xsl:template match="api">
		<api id="{@id}">
			<xsl:copy-of select="*" />
			<file name="{ddue:getFileName(@id)}" />
		</api>
	</xsl:template>

</xsl:stylesheet>
