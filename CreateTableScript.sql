CREATE TABLE SurveyQuestion (
    id INT PRIMARY KEY IDENTITY(1,1),
    question VARCHAR(255) NOT NULL
);

CREATE TABLE Surveys (
    id INT PRIMARY KEY IDENTITY(1,1),
    submit_date_time DATETIME NOT NULL
);

CREATE TABLE SurveysAnswer (
    id INT PRIMARY KEY IDENTITY(1,1),
    answer VARCHAR(255) NOT NULL,
    question_id INT NOT NULL,
    surveys_id INT NOT NULL,
    FOREIGN KEY (question_id) REFERENCES SurveyQuestion(id),
    FOREIGN KEY (surveys_id) REFERENCES Surveys(id)
);
