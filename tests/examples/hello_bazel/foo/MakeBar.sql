CREATE PROC EDURE [foo].[MakeBar]
AS BEGIN
    INSERT INTO [Foo].Bar (Name)
    VALUES ('yay')

    SELECT * from What.Why
END
