/*
sp_help ODIN_ITEM_VW

DROP VIEW ODIN_ITEM_VW
SELECT * FROM ODIN_ITEM_VW WHERE INV_ITEM_ID = 'ST5525'
*/
CREATE VIEW ODIN_ITEM_VW AS
	
	SELECT DISTINCT
		   MASTER_ITEM_TBL.INV_ITEM_ID,
           dbo.Odin_RetrieveSellOnFlag(MASTER_ITEM_TBL.INV_ITEM_ID, 'ALL POSTERS') AS SELL_ON_ALL_POSTERS,
           dbo.Odin_RetrieveSellOnFlag(MASTER_ITEM_TBL.INV_ITEM_ID, 'AMAZON') AS SELL_ON_AMAZON,
           dbo.Odin_RetrieveSellOnFlag(MASTER_ITEM_TBL.INV_ITEM_ID, 'AMAZON SELLER CENTRAL') AS SELL_ON_AMAZON_SELLER_CENTRAL,
           dbo.Odin_RetrieveSellOnFlag(MASTER_ITEM_TBL.INV_ITEM_ID, 'FANATICS') AS SELL_ON_FANATICS,
           dbo.Odin_RetrieveSellOnFlag(MASTER_ITEM_TBL.INV_ITEM_ID, 'GUITAR CENTER') AS SELL_ON_GUITAR_CENTER,
           dbo.Odin_RetrieveSellOnFlag(MASTER_ITEM_TBL.INV_ITEM_ID, 'HAYNEEDLE') AS SELL_ON_HAYNEEDLE,
           dbo.Odin_RetrieveSellOnFlag(MASTER_ITEM_TBL.INV_ITEM_ID, 'TARGET') AS SELL_ON_TARGET,
           dbo.Odin_RetrieveSellOnFlag(MASTER_ITEM_TBL.INV_ITEM_ID, 'WALMART') AS SELL_ON_WALMART,
           dbo.Odin_RetrieveSellOnFlag(MASTER_ITEM_TBL.INV_ITEM_ID, 'WAYFAIR') AS SELL_ON_WAYFAIR,
		   dbo.Odin_RetrieveItemLanguages(MASTER_ITEM_TBL.INV_ITEM_ID) AS [LANGUAGE],
		   dbo.Odin_RetrieveItemTerritories(MASTER_ITEM_TBL.INV_ITEM_ID) AS TERRITORY,
		   dbo.Odin_RetrieveProductIdTranslations(MASTER_ITEM_TBL.INV_ITEM_ID) AS PRODUCT_ID_TRANSLATION,
		   dbo.Odin_RetrieveBillOfMaterials(MASTER_ITEM_TBL.INV_ITEM_ID) AS	BILL_OF_MATERIALS,
		   AMAZON_ITEM_ATTRIBUTES.ASIN AS ECOMMERCE_ASIN,
		   AMAZON_ITEM_ATTRIBUTES.ITEM_NAME AS ECOMMERCE_ITEM_NAME,
		   AMAZON_ITEM_ATTRIBUTES.MODEL_NAME AS ECOMMERCE_MODEL_NAME,
		   AMAZON_ITEM_ATTRIBUTES.PRODUCT_CATEGORY AS ECOMMERCE_PRODUCT_CATEGORY,
		   AMAZON_ITEM_ATTRIBUTES.PRODUCT_SUBCATEGORY AS ECOMMERCE_PRODUCT_SUBCATEGORY,
		   AMAZON_ITEM_ATTRIBUTES.BULLET_1 AS ECOMMERCE_BULLET_1,
	       AMAZON_ITEM_ATTRIBUTES.BULLET_2 AS ECOMMERCE_BULLET_2,
		   AMAZON_ITEM_ATTRIBUTES.BULLET_3 AS ECOMMERCE_BULLET_3,
		   AMAZON_ITEM_ATTRIBUTES.BULLET_4 AS ECOMMERCE_BULLET_4,
		   AMAZON_ITEM_ATTRIBUTES.BULLET_5 AS ECOMMERCE_BULLET_5,
		   AMAZON_ITEM_ATTRIBUTES.FULL_DESCRIPTION AS ECOMMERCE_FULL_DESCRIPTION,
		   AMAZON_ITEM_ATTRIBUTES.EXTERNAL_ID_TYPE AS ECOMMERCE_EXTERNAL_ID_TYPE,
		   AMAZON_ITEM_ATTRIBUTES.EXTERNAL_ID AS ECOMMERCE_EXTERNAL_ID,
		   AMAZON_ITEM_ATTRIBUTES.SEARCH_TERMS AS ECOMMERCE_SEARCH_TERMS,
		   AMAZON_ITEM_ATTRIBUTES.IMAGE_URL_1 AS ECOMMERCE_IMAGE_URL_1,
		   AMAZON_ITEM_ATTRIBUTES.IMAGE_URL_2 AS ECOMMERCE_IMAGE_URL_2,
		   AMAZON_ITEM_ATTRIBUTES.IMAGE_URL_3 AS ECOMMERCE_IMAGE_URL_3,
		   AMAZON_ITEM_ATTRIBUTES.IMAGE_URL_4 AS ECOMMERCE_IMAGE_URL_4,
		   AMAZON_ITEM_ATTRIBUTES.IMAGE_URL_5 AS ECOMMERCE_IMAGE_URL_5,
		   AMAZON_ITEM_ATTRIBUTES.SIZE AS ECOMMERCE_SIZE,
		   AMAZON_ITEM_ATTRIBUTES.COST AS ECOMMERCE_COST,
		   AMAZON_ITEM_ATTRIBUTES.MSRP AS ECOMMERCE_MSRP,
		   AMAZON_ITEM_ATTRIBUTES.MANUFACTURER_NAME AS ECOMMERCE_MANUFACTURER_NAME,
		   AMAZON_ITEM_ATTRIBUTES.COUNTRY_OF_ORIGIN AS ECOMMERCE_COUNTRY_OF_ORIGIN,
		   AMAZON_ITEM_ATTRIBUTES.LENGTH AS ECOMMERCE_LENGTH,
		   AMAZON_ITEM_ATTRIBUTES.HEIGHT AS ECOMMERCE_HEIGHT,
		   AMAZON_ITEM_ATTRIBUTES.WIDTH AS ECOMMERCE_WIDTH,
		   AMAZON_ITEM_ATTRIBUTES.WEIGHT AS ECOMMERCE_WEIGHT,
		   AMAZON_ITEM_ATTRIBUTES.PACKAGE_LENGTH AS ECOMMERCE_PACKAGE_LENGTH,
		   AMAZON_ITEM_ATTRIBUTES.PACKAGE_HEIGHT AS ECOMMERCE_PACKAGE_HEIGHT,
		   AMAZON_ITEM_ATTRIBUTES.PACKAGE_WIDTH AS ECOMMERCE_PACKAGE_WIDTH,
		   AMAZON_ITEM_ATTRIBUTES.PACKAGE_WEIGHT AS ECOMMERCE_PACKAGE_WEIGHT,
		   AMAZON_ITEM_ATTRIBUTES.PAGE_COUNT AS ECOMMERCE_PAGE_COUNT,
		   AMAZON_ITEM_ATTRIBUTES.GENERIC_KEYWORDS AS ECOMMERCE_GENERIC_KEYWORDS,
		   AMAZON_ITEM_ATTRIBUTES.COMPONENTS AS ECOMMERCE_COMPONENTS,
		   AMAZON_ITEM_ATTRIBUTES.UPC_OVERRIDE AS ECOMMERCE_UPC,
		   BU_ITEMS_INV.COUNTRY_IST_ORIGIN,
		   BU_ITEMS_INV.CURRENT_COST AS DACUSD,
		   BU_ITEMS_INV.SOURCE_CODE AS MFG_SOURCE,
		   BU_ITEMS_INV_CAD.CURRENT_COST AS DACCAD,
		   INV_ITEMS.HARMONIZED_CD,
		   INV_ITEMS.INV_ITEM_COLOR,
		   INV_ITEMS.INV_ITEM_HEIGHT,
		   INV_ITEMS.INV_ITEM_LENGTH,
		   INV_ITEMS.INV_ITEM_WEIGHT,
		   INV_ITEMS.INV_ITEM_WIDTH,
		   INV_ITEMS.UPC_ID,
		   ITEM_ATTRIB_EX.IMAGE_FILE_NAME,
		   ITEM_ATTRIB_EX.ALT_IMAGE_FILE_1,
		   ITEM_ATTRIB_EX.ALT_IMAGE_FILE_2,
		   ITEM_ATTRIB_EX.ALT_IMAGE_FILE_3,
		   ITEM_ATTRIB_EX.ALT_IMAGE_FILE_4,
		   ITEM_ATTRIB_EX.CASEPACK_HEIGHT,
		   ITEM_ATTRIB_EX.CASEPACK_LENGTH,
		   ITEM_ATTRIB_EX.CASEPACK_QTY,
		   ITEM_ATTRIB_EX.CASEPACK_UPC,
		   ITEM_ATTRIB_EX.CASEPACK_WIDTH,
		   ITEM_ATTRIB_EX.CASEPACK_WEIGHT,
		   ITEM_ATTRIB_EX.DIRECT_IMPORT,
		   ITEM_ATTRIB_EX.INNERPACK_HEIGHT,
		   ITEM_ATTRIB_EX.INNERPACK_LENGTH,
		   ITEM_ATTRIB_EX.INNERPACK_QTY,
		   ITEM_ATTRIB_EX.INNERPACK_UPC,
		   ITEM_ATTRIB_EX.INNERPACK_WIDTH,
		   ITEM_ATTRIB_EX.INNERPACK_WEIGHT,
		   ITEM_ATTRIB_EX.LICENSE_BEGIN_DATE,
		   ITEM_ATTRIB_EX.PRINT_ON_DEMAND,
		   ITEM_ATTRIB_EX.PROD_FORMAT,
		   ITEM_ATTRIB_EX.PROD_GROUP,
		   ITEM_ATTRIB_EX.PROD_LINE,
		   ITEM_ATTRIB_EX.SAT_CODE,
		   ITEM_ATTRIB_EX.SELL_ON_ECOM AS SELL_ON_ECOMMERCE,
		   ITEM_ATTRIB_EX.SELL_ON_WEB,
		   ITEM_ATTRIB_EX.WEBSITE_PRICE,
		   ITEM_WEB_INFO.ATTRIBUTE_SET,
		   ITEM_WEB_INFO.CATEGORY,
		   ITEM_WEB_INFO.COPYRIGHT,
		   ITEM_WEB_INFO.ITEM_KEYWORDS,
		   ITEM_WEB_INFO.IN_STOCK_DATE,
		   ITEM_WEB_INFO.LICENSE,
		   ITEM_WEB_INFO.META_DESCRIPTION,
		   ITEM_WEB_INFO.NEWCATEGORY,
		   ITEM_WEB_INFO.NEW_DATE,
		   ITEM_WEB_INFO.ON_SITE,
		   ITEM_WEB_INFO.PROD_QTY,
		   ITEM_WEB_INFO.PROPERTY,
		   ITEM_WEB_INFO.SHORT_DESC,
		   ITEM_WEB_INFO.SIZE,
		   ITEM_WEB_INFO.TITLE,
		   MASTER_ITEM_TBL.CM_GROUP,
		   MASTER_ITEM_TBL.INV_ITEM_GROUP,
		   MASTER_ITEM_TBL.INV_PROD_FAM_CD,
		   MASTER_ITEM_TBL.ITEM_FIELD_C2 AS PSSTATUS,
		   PROD_ITEM.PROD_CATEGORY,
		   PROD_ITEM.PROD_FIELD_C10_A AS ISBN,
		   PROD_ITEM.PROD_FIELD_C10_C AS GPC,
		   PROD_ITEM.PROD_FIELD_C30_A AS EAN,
		   PROD_ITEM.PROD_FIELD_C30_B AS STATS_CODE,
		   PROD_ITEM.PROD_FIELD_C30_C AS UDEX,
		   PROD_PGRP_LNK_ACCT.PRODUCT_GROUP AS ACCOUNTING_GROUP,
		   PROD_PGRP_LNK_PRC.PRODUCT_GROUP AS PRICING_GROUP,
		   PROD_PRICE_CAN.LIST_PRICE AS LIST_PRICE_CAN,
		   PROD_PRICE_CAN.MFG_SUG_RTL_PRC AS MSRP_CAN,
		   PROD_PRICE_MXN.LIST_PRICE AS LIST_PRICE_MXN,
		   PROD_PRICE_MXN.MFG_SUG_RTL_PRC AS MSRP_MXN,
		   PROD_PRICE_USD.LIST_PRICE AS LIST_PRICE_USD,
		   PROD_PRICE_USD.MFG_SUG_RTL_PRC AS MSRP_USD,
		   PURCH_ITEM_ATTR.PRICE_LIST AS STANDARD_COST,
		   DESCR60,
		   DUTY
	FROM PS_MASTER_ITEM_TBL MASTER_ITEM_TBL
		 INNER JOIN	PS_INV_ITEMS INV_ITEMS
			ON  INV_ITEMS.SETID = 'SHARE'
			AND INV_ITEMS.INV_ITEM_ID = MASTER_ITEM_TBL.INV_ITEM_ID
			AND EFFDT = (SELECT MAX(EFFDT)
						 FROM PS_INV_ITEMS
						 WHERE SETID = 'SHARE'
						   AND INV_ITEM_ID = MASTER_ITEM_TBL.INV_ITEM_ID
						   AND EFFDT <= GETDATE())
		 LEFT OUTER JOIN PS_ITEM_ATTRIB_EX ITEM_ATTRIB_EX
			ON  ITEM_ATTRIB_EX.SETID = 'SHARE'
			AND ITEM_ATTRIB_EX.INV_ITEM_ID = MASTER_ITEM_TBL.INV_ITEM_ID
		 INNER JOIN PS_PROD_ITEM PROD_ITEM
			ON PROD_ITEM.PRODUCT_ID = MASTER_ITEM_TBL.INV_ITEM_ID
			AND PROD_ITEM.SETID = 'SHARE'
		 INNER JOIN PS_BU_ITEMS_INV BU_ITEMS_INV
			ON BU_ITEMS_INV.INV_ITEM_ID = MASTER_ITEM_TBL.INV_ITEM_ID
			AND BU_ITEMS_INV.BUSINESS_UNIT = 'TRUS1'
		 LEFT JOIN PS_ITEM_WEB_INFO ITEM_WEB_INFO
			ON ITEM_WEB_INFO.INV_ITEM_ID = MASTER_ITEM_TBL.INV_ITEM_ID
		 LEFT JOIN PS_PROD_PGRP_LNK PROD_PGRP_LNK
			ON PROD_PGRP_LNK.PRODUCT_ID = MASTER_ITEM_TBL.INV_ITEM_ID
			AND PROD_PGRP_LNK.SETID = 'SHARE'
		 LEFT JOIN PS_PURCH_ITEM_ATTR PURCH_ITEM_ATTR
			ON PURCH_ITEM_ATTR.INV_ITEM_ID = MASTER_ITEM_TBL.INV_ITEM_ID
			AND PURCH_ITEM_ATTR.SETID = 'SHARE'
		 LEFT JOIN PS_FXD_BIN_LOC_INV FXD_BIN_LOC_INV
			ON FXD_BIN_LOC_INV.INV_ITEM_ID = MASTER_ITEM_TBL.INV_ITEM_ID
		 LEFT JOIN PS_AMAZON_ITEM_ATTRIBUTES AMAZON_ITEM_ATTRIBUTES
			ON AMAZON_ITEM_ATTRIBUTES.INV_ITEM_ID = MASTER_ITEM_TBL.INV_ITEM_ID
		LEFT JOIN PS_PROD_PRICE PROD_PRICE_CAN 
			ON PROD_PRICE_CAN.PRODUCT_ID = MASTER_ITEM_TBL.INV_ITEM_ID
			 AND PROD_PRICE_CAN.BUSINESS_UNIT_IN='TRCN1' 
			 AND PROD_PRICE_CAN.CURRENCY_CD='CAD' 
			 AND PROD_PRICE_CAN.SETID = 'SHARE' 
			 AND PROD_PRICE_CAN.UNIT_OF_MEASURE = 'EA'
		LEFT JOIN PS_PROD_PRICE PROD_PRICE_MXN 
			ON PROD_PRICE_MXN.PRODUCT_ID = MASTER_ITEM_TBL.INV_ITEM_ID
			 AND PROD_PRICE_MXN.BUSINESS_UNIT_IN='TRUS1' 
			 AND PROD_PRICE_MXN.CURRENCY_CD='MXN' 
			 AND PROD_PRICE_MXN.SETID = 'SHARE' 
			 AND PROD_PRICE_MXN.UNIT_OF_MEASURE = 'EA'
		LEFT JOIN PS_PROD_PRICE PROD_PRICE_USD 
			ON PROD_PRICE_USD.PRODUCT_ID = MASTER_ITEM_TBL.INV_ITEM_ID
			 AND PROD_PRICE_USD.BUSINESS_UNIT_IN='TRUS1' 
			 AND PROD_PRICE_USD.CURRENCY_CD='USD' 
			 AND PROD_PRICE_USD.SETID = 'SHARE' 
			 AND PROD_PRICE_USD.UNIT_OF_MEASURE = 'EA'		
		LEFT JOIN PS_BU_ITEMS_INV BU_ITEMS_INV_CAD 
			ON BU_ITEMS_INV_CAD.INV_ITEM_ID = MASTER_ITEM_TBL.INV_ITEM_ID
			 AND BU_ITEMS_INV_CAD.BUSINESS_UNIT='TRCN1' 
		LEFT JOIN PS_PROD_PGRP_LNK PROD_PGRP_LNK_ACCT
			ON PROD_PGRP_LNK_ACCT.PRODUCT_ID = MASTER_ITEM_TBL.INV_ITEM_ID
			 AND PROD_PGRP_LNK_ACCT.PROD_GRP_TYPE='ACCT' 
			 AND PROD_PGRP_LNK_ACCT.SETID='SHARE' 
		LEFT JOIN PS_PROD_PGRP_LNK PROD_PGRP_LNK_PRC
			ON PROD_PGRP_LNK_PRC.PRODUCT_ID = MASTER_ITEM_TBL.INV_ITEM_ID
			 AND PROD_PGRP_LNK_PRC.PROD_GRP_TYPE='PRC' 
			 AND PROD_PGRP_LNK_PRC.SETID='SHARE' 
	WHERE MASTER_ITEM_TBL.SETID = 'SHARE'
	 
GRANT SELECT ON ODIN_ITEM_VW TO Odin