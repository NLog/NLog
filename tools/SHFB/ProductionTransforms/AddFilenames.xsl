<?xml version="1.0"?>
<xsl:stylesheet
	xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
	xmlns:msxsl="urn:schemas-microsoft-com:xslt"
	xmlns:ddue="urn:ddue-extensions"
	version="2.0">

	<xsl:param name="namingMethod" select="'Guid'"/>

	<msxsl:script language="C#" implements-prefix="ddue">
		<msxsl:assembly name="System.Core" />
		<msxsl:using namespace="System.Collections.Generic" />
		<msxsl:using namespace="System.Globalization" />
		<msxsl:using namespace="System.Security.Cryptography" />
		<msxsl:using namespace="System.Text.RegularExpressions" />
		<![CDATA[
			// This is used to generate GUID filenames (MD5 hashes in GUID form)
			private static HashAlgorithm md5 = HashAlgorithm.Create("MD5");

			// The reflection file can contain tens of thousands of entries for large assemblies.  HashSet<T> is much
			// faster at lookups than List<T>.
			private static HashSet<string> filenames = new HashSet<string>();

			private static Regex reInvalidChars = new Regex("[ :.`#<>*?]");

			// Convert a member ID to a filename based on the given naming method
			public static string GetFileName(string id, string namingMethod)
			{
					string memberName, newName;
					bool duplicate;
					int idx;

					if(namingMethod == "Guid")
					{
							byte[] input = Encoding.UTF8.GetBytes(id);
							byte[] output = md5.ComputeHash(input);
							Guid guid = new Guid(output);
							return guid.ToString();
					}

					memberName = id;

					// Remove parameters
					idx = memberName.IndexOf('(');

					if(idx != -1)
							memberName = memberName.Substring(0, idx);

					// Replace invalid filename characters with an underscore if member names are used as the filenames
					if(namingMethod == "MemberName")
							newName = memberName = reInvalidChars.Replace(memberName, "_");
					else
							newName = memberName;

					idx = 0;

					do
					{
							// Hash codes can be used to shorten extremely long type and member names
							if(namingMethod == "HashedMemberName")
									newName = String.Format(CultureInfo.InvariantCulture, "{0:X}", newName.GetHashCode());

							// Check for a duplicate (i.e. an overloaded member).  These will be made unique by adding a
							// counter to the end of the name.
							duplicate = filenames.Contains(newName);

							// VS2005/Hana style bug (probably fixed).  Overloads pages sometimes result in a duplicate
							// reflection file entry and we need to ignore it.
							if(duplicate)
									if(id.StartsWith("Overload:", StringComparison.Ordinal))
										  duplicate = false;
									else
									{
											idx++;
											newName = String.Format(CultureInfo.InvariantCulture, "{0}_{1}", memberName, idx);
									}

					} while(duplicate);

					// Log duplicates that had unique names created
					if(idx != 0)
							Console.WriteLine("    Unique name {0} generated for {1}", newName, id);

					filenames.Add(newName);

					return newName;
			}
		]]>
	</msxsl:script>

	<xsl:output indent="yes" encoding="UTF-8" />

	<xsl:template match="/">
		<reflection>
			<xsl:apply-templates select="/reflection/assemblies" />
			<xsl:apply-templates select="/reflection/apis" />
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
			<file name="{ddue:GetFileName(@id, $namingMethod)}" />
		</api>
	</xsl:template>

</xsl:stylesheet>
