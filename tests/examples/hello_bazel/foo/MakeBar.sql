CREATE PROCEDURE [foo].[MakeBar]
AS BEGIN
    INSERT INTO [foo].Bar (Name)
    VALUES ('yay')

    SELECT * from Foo.Bar
END
