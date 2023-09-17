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
SET FanYi=FanYi %CUR_PATH%\Language\oldfanyi\UIText_TChinese.txt
SET COL_CN=COL_CN 1 
SET COL_Dst=COL_Dst 3 
SET FindStr=FindStr 鳞甲49级
SET PrefabInfo=PrefabInfo %CLIENT_TB_DIR%\PrefabLangInfo.txt
SET PARGMA1=%Mode% %SRC_TB_DIR% %DST_TB_DIR% %LangInfo% %LangInfo% %AddChinese% %RecordFile%
SET PARGMA2=%DstLang% %FanYi% %COL_CN% %COL_Dst% %PrefabInfo%
SET TIP_DIR=%CUR_PATH%\shengwang\Assets\Resources\Tips

%EXE% %PARGMA1% %PARGMA2%
PAUSE