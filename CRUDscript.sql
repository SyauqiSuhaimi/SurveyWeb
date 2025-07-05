INSERT INTO SurveyQuestion (question, type)
VALUES ('What is your gender?', 'radio');

UPDATE SurveyQuestion
SET errormsg = 'Age must be between 1-100'
WHERE id = 6;

SELECT * FROM SurveysAnswer

ALTER TABLE SurveyQuestion
ADD type NVARCHAR(100) DEFAULT 'text',
status TINYINT DEFAULT 1 ;

delete from SurveyQuestion
where id in (1,2)

delete from Surveys