-- Cria��o do banco de dados
CREATE DATABASE CatFactsExplorerDb;
GO

-- Selecionar o banco de dados
USE CatFactsExplorerDb;
GO

-- Cria��o da tabela CatFacts
CREATE TABLE CatFacts (
    Id INT PRIMARY KEY IDENTITY,       -- Identificador �nico
    Fact NVARCHAR(500) NOT NULL,       -- Fato sobre gatos
    Length INT NOT NULL                -- Comprimento do fato
);
GO