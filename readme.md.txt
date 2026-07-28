Для запуска необходимо будет установить пакеты nuget, vusial studio 2023 или новее, .net 9 версии

Для запуска необходимо python 3.11

Перейти в папку, где будет находится сервер
установить виртуальное окружение
Если на линукс
python3 -m venv venv
Если на виндовс
python -m venv venv

Активировать виртуальное окружение
Для линукс:
source venv/bin/activate
Для виндовс:
.\venv\Scripts\activate
Установить зависимости
pip install -r requirements.txt

Запустить при активированном окружении

На линукс
python3 main.py server main

На виндовс
python main.py server main