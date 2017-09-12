-- Run under priviliged user (sys/system)

create user TEST_NH_USER
  profile DEFAULT
  identified by test;
grant connect to TEST_NH_USER;
grant resource to TEST_NH_USER;
grant create procedure to TEST_NH_USER;
grant create sequence to TEST_NH_USER;
grant create session to TEST_NH_USER;
grant create table to TEST_NH_USER;
grant create trigger to TEST_NH_USER;
grant create view to TEST_NH_USER;
grant unlimited tablespace to TEST_NH_USER;

create table TEST_NH_USER.T_PRODUCT
(
  id_product NUMBER(20) NOT NULL,
  code       NVARCHAR2(255) NOT NULL,
  name       XMLTYPE
);

alter table TEST_NH_USER.T_PRODUCT
  add primary key (ID_PRODUCT);
  
alter table TEST_NH_USER.T_PRODUCT 
  add unique (CODE);
;

CREATE INDEX TEST_NH_USER.IX_PROD_NAME
  ON TEST_NH_USER.T_PRODUCT  (name) 
  INDEXTYPE IS XDB.XMLIndex  
  PARAMETERS ('
  PATHS 
    (INCLUDE 
      (
          /MultiCulturalString/*
      )
    NAMESPACE MAPPING ( xmlns="http://custis.ru/i18n" )
    )
  PATH TABLE T_IX_PROD_NAME_PATH_TABLE   
  PATH ID    INDEX IX_PROD_NAME_PATH_ID   
  ORDER KEY  INDEX IX_PROD_NAME_ORDER_KEY 
  VALUE      INDEX IX_PROD_NAME_VALUE     
  ASYNC (SYNC ON COMMIT)
  ');

CREATE OR REPLACE FUNCTION TEST_NH_USER.MCS_GET_STRING
(
  a_mcs            XMLTYPE,
  a_locale         VARCHAR2,
  a_fallback_chain VARCHAR2 := NULL
)
RETURN VARCHAR2
DETERMINISTIC
AS
  l_value VARCHAR2(4000 CHAR);
  l_list_separator CONSTANT VARCHAR2(1 CHAR) := ',';
  l_separator_position INTEGER;
BEGIN
  IF (a_mcs IS NULL) THEN
    RETURN NULL;
  END IF;
  IF (a_locale IS NULL) THEN
    raise_application_error(-20000, 'NULL a_locale parameter');
  END IF;
  l_separator_position := INSTR(a_fallback_chain, l_list_separator);
  IF (a_fallback_chain IS NOT NULL AND a_locale <> CASE WHEN l_separator_position > 0 THEN SUBSTR(a_fallback_chain, 0, l_separator_position - 1) ELSE a_fallback_chain END) THEN
    raise_application_error(-20000, 'a_locale and a_fallback_chain mismatch');
  END IF;



  SELECT XMLCast(
    XMLQuery('declare namespace i18n="http://custis.ru/i18n";
        for $mcs_locale in $mcs/i18n:MultiCulturalString/*
        where $mcs_locale/name() = $locale
        return $mcs_locale/text()'
      PASSING a_mcs AS "mcs", a_locale AS "locale" RETURNING CONTENT)
    AS VARCHAR2(4000 CHAR))
  INTO l_value
  FROM dual;
  
  DECLARE
    l_next_fallback_chain VARCHAR2(100 CHAR) := CASE WHEN l_separator_position > 0 THEN SUBSTR(a_fallback_chain, l_separator_position + 1) ELSE NULL END;
    l_local_separator_position INTEGER := INSTR(l_next_fallback_chain, l_list_separator);
    l_next_locale VARCHAR2(100 CHAR) := CASE WHEN l_local_separator_position > 0 THEN SUBSTR(l_next_fallback_chain, 0, l_local_separator_position - 1) ELSE l_next_fallback_chain END;
  BEGIN
    IF (a_fallback_chain IS NOT NULL AND l_value IS NULL AND l_next_locale IS NOT NULL) THEN
        l_value := MCS_GET_STRING(a_mcs => a_mcs, 
                                  a_locale => l_next_locale, 
                                  a_fallback_chain => l_next_fallback_chain);
    END IF;  
  END;
  RETURN l_value;
END;
/

CREATE INDEX TEST_NH_USER.IX_PRODNAME_RU_NOFALLBACK
  ON TEST_NH_USER.T_PRODUCT (TEST_NH_USER.mcs_get_string(name, 'ru', null));
  
CREATE INDEX TEST_NH_USER.IX_PRODNAME_RU_STDFALLBACK
  ON TEST_NH_USER.T_PRODUCT (TEST_NH_USER.mcs_get_string(name, 'ru', 'ru,en'));

CREATE INDEX TEST_NH_USER.IX_PRODNAME_RURU_STDFALLBACK
  ON TEST_NH_USER.T_PRODUCT (TEST_NH_USER.mcs_get_string(name, 'ru-RU', 'ru-RU,ru,en'));

CREATE INDEX TEST_NH_USER.IX_PRODNAME_EN_NOFALLBACK
  ON TEST_NH_USER.T_PRODUCT (TEST_NH_USER.mcs_get_string(name, 'en', null));
  
CREATE INDEX TEST_NH_USER.IX_PRODNAME_EN_STDFALLBACK
  ON TEST_NH_USER.T_PRODUCT (TEST_NH_USER.mcs_get_string(name, 'en', 'en'));

CREATE INDEX TEST_NH_USER.IX_PRODNAME_ENUS_STDFALLBACK
  ON TEST_NH_USER.T_PRODUCT (TEST_NH_USER.mcs_get_string(name, 'en-US', 'en-US,en'));

