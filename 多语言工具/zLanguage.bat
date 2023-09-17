@echo off
REM Mode:1提取中文，2翻译内容添加到LangInfo中,4拷贝文件，8翻译替换到表中
SET CUR_PATH=%~dp0
SET EXE=%CUR_PATH%\Tools\LangueTool\Language.exe
SET Mode=Mode 1
SET BUNDLE_DIR=%CUR_PATH%\shengwang\Assets\BundleData
SET CLIENT_TB_DIR=%BUNDLE_DIR%\Tables
SET SRC_TB_DIR=SRC_TB_DIR %CLIENT_TB_DIR%
SET DST_TB_DIR=DST_TB_DIR %CLIENT_TB_DIR% 
SET LangInfo=LangInfo %CUR_PATH%\Language\langInfo.txt
SET AddChinese=AddChinese %CUR_PATH%\Language\add_chinese.txt 
SET RecordFile=RecordFile %CUR_PATH%\Language\record_cnfile.txt 
SET DstLang=DstLang TCHINESE 
SET FanYi=FanYi %CUR_PATH%\Language\newfanyi\UIText_TChinese.txt
SET COL_CN=COL_CN 0
SET COL_Dst=COL_Dst 1 
SET FindStr=FindStr 鳞甲49级
SET PrefabInfo=PrefabInfo %CLIENT_TB_DIR%\PrefabLangInfo.txt
SET PARGMA1=%Mode% %SRC_TB_DIR% %DST_TB_DIR% %LangInfo% %LangInfo% %AddChinese% %RecordFile%
SET PARGMA2=%DstLang% %FanYi% %COL_CN% %COL_Dst% %PrefabInfo%
SET TIP_DIR=%CUR_PATH%\shengwang\Assets\Resources\Tips

%EXE% %PARGMA1% %PARGMA2%

SET Mode=Mode 14

SET DstLang=DstLang TCHINESE 
SET FanYi=FanYi %CUR_PATH%\Language\newfanyi\UIText_TChinese.txt
SET CLIENT_TB_DIR=%BUNDLE_DIR%\Tables_tchinese
SET DST_TB_DIR=DST_TB_DIR %CLIENT_TB_DIR%
SET PrefabInfo=PrefabInfo %CLIENT_TB_DIR%\PrefabLangInfo.txt
SET PARGMA1=%Mode% %SRC_TB_DIR% %DST_TB_DIR% %LangInfo% %LangInfo% %AddChinese% %RecordFile%
SET PARGMA2=%DstLang% %FanYi% %COL_CN% %COL_Dst% %PrefabInfo%
%EXE% %PARGMA1% %PARGMA2%

SET DstLang=DstLang ENGLISH 
SET FanYi=FanYi %CUR_PATH%\Language\newfanyi\UIText_English.txt
SET CLIENT_TB_DIR=%BUNDLE_DIR%\Tables_english
SET DST_TB_DIR=DST_TB_DIR %CLIENT_TB_DIR%
SET PrefabInfo=PrefabInfo %CLIENT_TB_DIR%\PrefabLangInfo.txt
SET PARGMA1=%Mode% %SRC_TB_DIR% %DST_TB_DIR% %LangInfo% %LangInfo% %AddChinese% %RecordFile%
SET PARGMA2=%DstLang% %FanYi% %COL_CN% %COL_Dst% %PrefabInfo%
%EXE% %PARGMA1% %PARGMA2%

SET DstLang=DstLang KOREA 
SET FanYi=FanYi %CUR_PATH%\Language\newfanyi\UIText_Korea.txt
SET CLIENT_TB_DIR=%BUNDLE_DIR%\Tables_korea
SET DST_TB_DIR=DST_TB_DIR %CLIENT_TB_DIR%
SET PrefabInfo=PrefabInfo %CLIENT_TB_DIR%\PrefabLangInfo.txt
SET PARGMA1=%Mode% %SRC_TB_DIR% %DST_TB_DIR% %LangInfo% %LangInfo% %AddChinese% %RecordFile%
SET PARGMA2=%DstLang% %FanYi% %COL_CN% %COL_Dst% %PrefabInfo%
%EXE% %PARGMA1% %PARGMA2%

SET DstLang=DstLang THAILAND 
SET FanYi=FanYi %CUR_PATH%\Language\newfanyi\UIText_Thailand.txt
SET CLIENT_TB_DIR=%BUNDLE_DIR%\Tables_thailand
SET DST_TB_DIR=DST_TB_DIR %CLIENT_TB_DIR%
SET PrefabInfo=PrefabInfo %CLIENT_TB_DIR%\PrefabLangInfo.txt
SET PARGMA1=%Mode% %SRC_TB_DIR% %DST_TB_DIR% %LangInfo% %LangInfo% %AddChinese% %RecordFile%
SET PARGMA2=%DstLang% %FanYi% %COL_CN% %COL_Dst% %PrefabInfo%
%EXE% %PARGMA1% %PARGMA2%

SET DstLang=DstLang VIETNAM 
SET FanYi=FanYi %CUR_PATH%\Language\newfanyi\UIText_Vietnam.txt
SET CLIENT_TB_DIR=%BUNDLE_DIR%\Tables_vietnam
SET DST_TB_DIR=DST_TB_DIR %CLIENT_TB_DIR%
SET PrefabInfo=PrefabInfo %CLIENT_TB_DIR%\PrefabLangInfo.txt
SET PARGMA1=%Mode% %SRC_TB_DIR% %DST_TB_DIR% %LangInfo% %LangInfo% %AddChinese% %RecordFile%
SET PARGMA2=%DstLang% %FanYi% %COL_CN% %COL_Dst% %PrefabInfo%
%EXE% %PARGMA1% %PARGMA2%

ECHO F|XCOPY /Y %BUNDLE_DIR%\Tables\Client\UpdateTips.txt %TIP_DIR%\UpdateTips.txt
ECHO F|XCOPY /Y %BUNDLE_DIR%\Tables_tchinese\Client\UpdateTips.txt %TIP_DIR%\UpdateTips_tchinese.txt
ECHO F|XCOPY /Y %BUNDLE_DIR%\Tables_english\Client\UpdateTips.txt %TIP_DIR%\UpdateTips_english.txt
ECHO F|XCOPY /Y %BUNDLE_DIR%\Tables_korea\Client\UpdateTips.txt %TIP_DIR%\UpdateTips_korea.txt
ECHO F|XCOPY /Y %BUNDLE_DIR%\Tables_vietnam\Client\UpdateTips.txt %TIP_DIR%\UpdateTips_vietnam.txt
ECHO F|XCOPY /Y %BUNDLE_DIR%\Tables_thailand\Client\UpdateTips.txt %TIP_DIR%\UpdateTips_thailand.txt


PAUSE