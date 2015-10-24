-- Tabla que registra los documentos que se han analizado
-- y la cantidad de letras en total que posee.
CREATE TABLE DOCUMENTOS (
	ID INTEGER PRIMARY KEY ASC,
	LETRAS INTEGER -- Total de letras en el documento, igual a la suma de cantidad en documentos_letras
);

-- Tabla que registra las letras que contenía cada documento analizado
-- y cuantas veces aparece la letra en el documento.
CREATE TABLE DOCUMENTOS_LETRAS (
	DOCUMENTO_ID INTEGER,
	LETRA_ID INTEGER,    -- Unicode del caracter
	CANTIDAD INTEGER, -- Cantidad de veces que aparece el caracter en el documento.
	PORCENTAJE REAL,  -- Porcentaje de veces que aparece el caracter en el documento.
	PRIMARY KEY (DOCUMENTO_ID ASC, LETRA_ID ASC)
);

CREATE TABLE IDIOMAS (
	ID INTEGER PRIMARY KEY ASC,
	NOMBRE TEXT NOT NULL UNIQUE,
	LETRAS INTEGER -- Total de letras registradas en el idioma, igual a la suma de cantidad en idiomas_letras
);

CREATE TABLE IDIOMAS_LETRAS (
	IDIOMA_ID INTEGER,
	LETRA_ID INTEGER,
	CANTIDAD INTEGER,
	PORCENTAJE REAL,
	PRIMARY KEY (IDIOMA_ID ASC, LETRA_ID ASC)
);

