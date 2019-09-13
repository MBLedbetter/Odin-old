SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

SET ANSI_PADDING ON
GO

CREATE TABLE [dbo].[ODIN_SHOPTRENDS_BRANDS]
(  
    [BRAND] VARCHAR(254)
)
GO

SET ANSI_PADDING OFF
GO

GRANT SELECT, INSERT ON ODIN_SHOPTRENDS_BRANDS TO Odin
/*

SELECT * FROM ODIN_SHOPTRENDS_BRANDS

INSERT INTO [ODIN_SHOPTRENDS_BRANDS] VALUES ('TMNT')
INSERT INTO [ODIN_SHOPTRENDS_BRANDS] VALUES ('The Walking Dead')
INSERT INTO [ODIN_SHOPTRENDS_BRANDS] VALUES ('Call of Duty')
INSERT INTO [ODIN_SHOPTRENDS_BRANDS] VALUES ('Super Hero Girls')
INSERT INTO [ODIN_SHOPTRENDS_BRANDS] VALUES ('Bob Ross')
INSERT INTO [ODIN_SHOPTRENDS_BRANDS] VALUES ('Harley Davidson')
INSERT INTO [ODIN_SHOPTRENDS_BRANDS] VALUES ('Riverdale')
INSERT INTO [ODIN_SHOPTRENDS_BRANDS] VALUES ('BT21')
INSERT INTO [ODIN_SHOPTRENDS_BRANDS] VALUES ('Assassins Creed')
INSERT INTO [ODIN_SHOPTRENDS_BRANDS] VALUES ('Boruto')
INSERT INTO [ODIN_SHOPTRENDS_BRANDS] VALUES ('Moana')
INSERT INTO [ODIN_SHOPTRENDS_BRANDS] VALUES ('Cuphead')
INSERT INTO [ODIN_SHOPTRENDS_BRANDS] VALUES ('Activision')
INSERT INTO [ODIN_SHOPTRENDS_BRANDS] VALUES ('Aggretsuko')
INSERT INTO [ODIN_SHOPTRENDS_BRANDS] VALUES ('Attack on Titan')
INSERT INTO [ODIN_SHOPTRENDS_BRANDS] VALUES ('DC Comics')
INSERT INTO [ODIN_SHOPTRENDS_BRANDS] VALUES ('Disney')
INSERT INTO [ODIN_SHOPTRENDS_BRANDS] VALUES ('Friends')
INSERT INTO [ODIN_SHOPTRENDS_BRANDS] VALUES ('Harry Potter')
INSERT INTO [ODIN_SHOPTRENDS_BRANDS] VALUES ('Marvel')
INSERT INTO [ODIN_SHOPTRENDS_BRANDS] VALUES ('Minecraft')
INSERT INTO [ODIN_SHOPTRENDS_BRANDS] VALUES ('MLB')
INSERT INTO [ODIN_SHOPTRENDS_BRANDS] VALUES ('My Hero Academia')
INSERT INTO [ODIN_SHOPTRENDS_BRANDS] VALUES ('NBA')
INSERT INTO [ODIN_SHOPTRENDS_BRANDS] VALUES ('Netflix')
INSERT INTO [ODIN_SHOPTRENDS_BRANDS] VALUES ('NFL')
INSERT INTO [ODIN_SHOPTRENDS_BRANDS] VALUES ('NHL')
INSERT INTO [ODIN_SHOPTRENDS_BRANDS] VALUES ('Nickelodeon')
INSERT INTO [ODIN_SHOPTRENDS_BRANDS] VALUES ('Ninja')
INSERT INTO [ODIN_SHOPTRENDS_BRANDS] VALUES ('Overwatch')
INSERT INTO [ODIN_SHOPTRENDS_BRANDS] VALUES ('Pokemon')
INSERT INTO [ODIN_SHOPTRENDS_BRANDS] VALUES ('Rick and Morty')
INSERT INTO [ODIN_SHOPTRENDS_BRANDS] VALUES ('Rolling Stone')
INSERT INTO [ODIN_SHOPTRENDS_BRANDS] VALUES ('RWBY')
INSERT INTO [ODIN_SHOPTRENDS_BRANDS] VALUES ('Sanrio')
INSERT INTO [ODIN_SHOPTRENDS_BRANDS] VALUES ('Sports Illustrated')
INSERT INTO [ODIN_SHOPTRENDS_BRANDS] VALUES ('Star Wars')
INSERT INTO [ODIN_SHOPTRENDS_BRANDS] VALUES ('Stranger Things')
INSERT INTO [ODIN_SHOPTRENDS_BRANDS] VALUES ('Toy Story')
INSERT INTO [ODIN_SHOPTRENDS_BRANDS] VALUES ('WWE')
INSERT INTO [ODIN_SHOPTRENDS_BRANDS] VALUES ('Yugioh')
INSERT INTO [ODIN_SHOPTRENDS_BRANDS] VALUES ('Yuri On Ice')

*/