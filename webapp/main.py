from dataclasses import dataclass, asdict
from flask import Flask, request, render_template, redirect
from flask_bootstrap import Bootstrap5
from storage import Storage, CourseInfo

app = Flask(__name__)
bootstrap = Bootstrap5(app)

storage = Storage()
all_course_info = storage.get_all_course_info()

APP_DOWNLOAD_LINK = "https://www.dropbox.com/scl/fi/xtld63x64ipluti7bqm22/SkyridingRaceLeaderboardsCompanionApp-1.0.2.0-Setup.exe?rlkey=dml108tbhxhgytgrvlo6dgz2p&st=vu19hsv8&dl=1"
ADDON_DOWNLOAD_LINK = "https://www.dropbox.com/scl/fi/cc6qw3nv5ylh8j4e41wqd/Addon0.2.zip?rlkey=k03ysa542zphmihixo0ev7r9f&st=g4rl4ozm&dl=1"


@dataclass
class CourseRecord:
    user_id: str
    course_id: str
    time_disp: str
    character_name: str
    race_name: str
    course_type: str


@dataclass
class CourseTimeDisplay:
    course_id: str
    user_id: str
    time_ms: int
    time_disp: str
    character_name: str


@app.route("/")
def index():
    storage = Storage()
    course_times = storage.get_all_course_records()
    course_records: list[CourseRecord] = []
    for course_time in course_times:
        course_record = CourseRecord(
            user_id=course_time.user_id,
            course_id=course_time.course_id,
            time_disp=format_time(course_time.time_ms),
            character_name=course_time.character_name,
            race_name=course_time.course_id,
            course_type="",
        )
        course_info = all_course_info.get(course_time.course_id)
        if course_info is not None:
            course_record.race_name = course_info.race_name
            course_record.course_type = course_info.course_type
        course_records.append(course_record)
    course_records.sort(key=lambda x: x.race_name)
    return render_template("index.html", course_records=course_records)


@app.route("/course_leaderboard", methods=["GET"])
def course_leaderboard():
    course_id = request.args.get("course_id")
    if course_id is None:
        return "Course ID couldn't be found", 404
    storage = Storage()
    course_times = storage.get_course_times(course_id)
    course_times_disp: list[CourseTimeDisplay] = []
    for course_time in course_times:
        course_time_disp = CourseTimeDisplay(
            **asdict(course_time), time_disp=format_time(course_time.time_ms)
        )
        course_times_disp.append(course_time_disp)
    course_info = all_course_info.get(course_id)
    if course_info is None:
        course_info = CourseInfo(course_id, course_id, "")
    return render_template(
        "leaderboard.html",
        course_info=course_info,
        course_times=course_times_disp,
    )


@app.route("/download", methods=["GET"])
def download():
    file_type = request.args.get("file")
    location = None
    if file_type == "app":
        location = APP_DOWNLOAD_LINK
    if file_type == "addon":
        location = ADDON_DOWNLOAD_LINK
    if location == None:
        return "Unknown download type", 404
    return redirect(location)


@app.route("/get_the_app")
def get_the_app():
    return render_template("get-the-app.html")


@app.route("/upload", methods=["POST"])
def upload_data():
    storage = Storage()
    result = request.get_json()
    print(result)
    battle_tag = result["battleTag"]
    for char_race_data in result["characterRaceData"]:
        character_name = char_race_data["characterName"]
        for course_time in char_race_data["courseTimes"]:
            course_id = course_time["courseId"]
            time_ms = course_time["timeMs"]
            storage.update_time(battle_tag, course_id, int(time_ms), character_name)
    storage.commit()
    return "", 200


def format_time(time_ms) -> str:
    time_s = time_ms / 1000
    return f"{round(time_s, 3):.3f}s"
