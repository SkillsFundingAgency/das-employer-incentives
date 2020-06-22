CREATE PROCEDURE [dbo].[AddOrUpdateAccount]
	@Id						BIGINT,
	@AccountLegalEntityId	BIGINT,
	@LegalEntityId			BIGINT,
	@LegalEntityName		VARCHAR(MAX)
AS
BEGIN
	SET NOCOUNT ON;

    IF NOT EXISTS (
		SELECT * 
		FROM Accounts 
		 WHERE Id = @Id 
		 AND AccountLegalEntityId = @AccountLegalEntityId
		 )
	BEGIN
		INSERT INTO Accounts
		(
			Id, 
			AccountLegalEntityId, 
			LegalEntityId, 
			LegalEntityName
		) 
		VALUES
		(
			@Id, 
			@AccountLegalEntityId, 
			@LegalEntityId, 
			@LegalEntityName
		)
	END
END
GO
