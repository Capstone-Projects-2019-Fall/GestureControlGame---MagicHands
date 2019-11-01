rm -r __pycache__*
rm -r build*
rm -r dist*
rm quang_tracker.spec*

source activate tensorflow_cpu
pyinstaller --onefile quang_tracker.py
cp haarcascade_frontalface_default.xml dist/