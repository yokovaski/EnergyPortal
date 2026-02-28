BEGIN;

ALTER TABLE public.settings
  ALTER COLUMN gas_price TYPE numeric(18,6) USING round(gas_price::numeric, 6),
  ALTER COLUMN high_usage_price_per_kwh TYPE numeric(18,6) USING round(high_usage_price_per_kwh::numeric, 6),
  ALTER COLUMN low_usage_price_per_kwh TYPE numeric(18,6) USING round(low_usage_price_per_kwh::numeric, 6),
  ALTER COLUMN high_redelivery_price_per_kwh TYPE numeric(18,6) USING round(high_redelivery_price_per_kwh::numeric, 6),
  ALTER COLUMN low_redelivery_price_per_kwh TYPE numeric(18,6) USING round(low_redelivery_price_per_kwh::numeric, 6),
  ALTER COLUMN electricity_delivery_price_per_month TYPE numeric(18,6) USING round(electricity_delivery_price_per_month::numeric, 6),
  ALTER COLUMN gas_delivery_price_per_month TYPE numeric(18,6) USING round(gas_delivery_price_per_month::numeric, 6);

COMMIT;
