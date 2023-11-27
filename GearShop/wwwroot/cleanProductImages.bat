@Echo Off
	Set "BoxIn=.\productImages"
	Set "Exclude=NoPhoto.png"

	FOR /F "usebackq delims=" %%f IN (`2^>nul Dir "%BoxIn%" /B /S /A:-D`) DO (
		If /I Not "%Exclude%"=="%%~nxf" Del /F /A "%%f"
	)
Exit /B