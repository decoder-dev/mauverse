from fastapi import FastAPI

import apps.schedule.routers.common as routers

app = FastAPI()
routers.init(app)
