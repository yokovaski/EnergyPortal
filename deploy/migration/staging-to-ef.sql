BEGIN;

DO $$
BEGIN
  IF to_regclass('public."AspNetUsers"') IS NULL THEN
    RAISE EXCEPTION 'Expected EF table public."AspNetUsers" does not exist. Run EF migrations first.';
  END IF;

  IF to_regclass('energycloud.aspnetusers') IS NULL THEN
    RAISE EXCEPTION 'Expected staging table energycloud.aspnetusers does not exist. Run pgloader first.';
  END IF;
END
$$;

TRUNCATE TABLE
  public."AspNetRoleClaims",
  public."AspNetUserClaims",
  public."AspNetUserLogins",
  public."AspNetUserRoles",
  public."AspNetUserTokens",
  public.hour_metrics,
  public.minute_metrics,
  public.settings,
  public.ten_second_metrics,
  public."AspNetRoles",
  public.raspberry_pis,
  public."AspNetUsers"
RESTART IDENTITY CASCADE;

INSERT INTO public."AspNetRoles" (id, name, normalized_name, concurrency_stamp)
SELECT id, name, normalizedname, concurrencystamp
FROM energycloud.aspnetroles;

INSERT INTO public."AspNetUsers" (
  id,
  user_name,
  normalized_user_name,
  email,
  normalized_email,
  email_confirmed,
  password_hash,
  security_stamp,
  concurrency_stamp,
  phone_number,
  phone_number_confirmed,
  two_factor_enabled,
  lockout_end,
  lockout_enabled,
  access_failed_count)
SELECT
  id,
  username,
  normalizedusername,
  email,
  normalizedemail,
  emailconfirmed,
  passwordhash,
  securitystamp,
  concurrencystamp,
  phonenumber,
  phonenumberconfirmed,
  twofactorenabled,
  lockoutend,
  lockoutenabled,
  accessfailedcount
FROM energycloud.aspnetusers;

INSERT INTO public."AspNetRoleClaims" (id, role_id, claim_type, claim_value)
SELECT id, roleid, claimtype, claimvalue
FROM energycloud.aspnetroleclaims;

INSERT INTO public."AspNetUserClaims" (id, user_id, claim_type, claim_value)
SELECT id, userid, claimtype, claimvalue
FROM energycloud.aspnetuserclaims;

INSERT INTO public."AspNetUserLogins" (login_provider, provider_key, provider_display_name, user_id)
SELECT loginprovider, providerkey, providerdisplayname, userid
FROM energycloud.aspnetuserlogins;

INSERT INTO public."AspNetUserRoles" (user_id, role_id)
SELECT userid, roleid
FROM energycloud.aspnetuserroles;

INSERT INTO public."AspNetUserTokens" (user_id, login_provider, name, value)
SELECT userid, loginprovider, name, value
FROM energycloud.aspnetusertokens;

INSERT INTO public.raspberry_pis (id, created, updated, rpi_key, user_id) OVERRIDING SYSTEM VALUE
SELECT id, created, updated, rpikey, userid
FROM energycloud.raspberrypis;

INSERT INTO public.settings (
  id,
  time_zone_id,
  solar_system,
  show_day_name,
  user_id,
  gas_price,
  high_usage_price_per_kwh,
  low_usage_price_per_kwh,
  high_redelivery_price_per_kwh,
  low_redelivery_price_per_kwh,
  electricity_delivery_price_per_month,
  gas_delivery_price_per_month)
SELECT
  id,
  timezoneid,
  solarsystem,
  showdayname,
  userid,
  round(gasprice::numeric, 6)::numeric(18,6),
  round(highusagepriceperkwh::numeric, 6)::numeric(18,6),
  round(lowusagepriceperkwh::numeric, 6)::numeric(18,6),
  round(highredeliverypriceperkwh::numeric, 6)::numeric(18,6),
  round(lowredeliverypriceperkwh::numeric, 6)::numeric(18,6),
  round(electricitydeliverypricepermonth::numeric, 6)::numeric(18,6),
  round(gasdeliverypricepermonth::numeric, 6)::numeric(18,6)
FROM energycloud.settings;

INSERT INTO public.hour_metrics (
  id,
  raspberry_pi_id,
  mode,
  usage_now,
  redelivery_now,
  solar_now,
  usage_total_high,
  redelivery_total_high,
  usage_total_low,
  redelivery_total_low,
  solar_total,
  usage_gas_now,
  usage_gas_total,
  created,
  updated)
SELECT
  id,
  raspberrypiid,
  mode,
  usagenow,
  redeliverynow,
  solarnow,
  usagetotalhigh,
  redeliverytotalhigh,
  usagetotallow,
  redeliverytotallow,
  solartotal,
  usagegasnow,
  usagegastotal,
  created,
  updated
FROM energycloud.hourmetrics;

INSERT INTO public.minute_metrics (
  id,
  raspberry_pi_id,
  mode,
  usage_now,
  redelivery_now,
  solar_now,
  usage_total_high,
  redelivery_total_high,
  usage_total_low,
  redelivery_total_low,
  solar_total,
  usage_gas_now,
  usage_gas_total,
  created,
  updated)
SELECT
  id,
  raspberrypiid,
  mode,
  usagenow,
  redeliverynow,
  solarnow,
  usagetotalhigh,
  redeliverytotalhigh,
  usagetotallow,
  redeliverytotallow,
  solartotal,
  usagegasnow,
  usagegastotal,
  created,
  updated
FROM energycloud.minutemetrics;

INSERT INTO public.ten_second_metrics (
  id,
  raspberry_pi_id,
  mode,
  usage_now,
  redelivery_now,
  solar_now,
  usage_total_high,
  redelivery_total_high,
  usage_total_low,
  redelivery_total_low,
  solar_total,
  usage_gas_now,
  usage_gas_total,
  created,
  updated)
SELECT
  id,
  raspberrypiid,
  mode,
  usagenow,
  redeliverynow,
  solarnow,
  usagetotalhigh,
  redeliverytotalhigh,
  usagetotallow,
  redeliverytotallow,
  solartotal,
  usagegasnow,
  usagegastotal,
  created,
  updated
FROM energycloud.tensecondmetrics;

SELECT setval(pg_get_serial_sequence('public."AspNetRoleClaims"', 'id'), COALESCE((SELECT MAX(id) FROM public."AspNetRoleClaims"), 1), true);
SELECT setval(pg_get_serial_sequence('public."AspNetUserClaims"', 'id'), COALESCE((SELECT MAX(id) FROM public."AspNetUserClaims"), 1), true);
SELECT setval(pg_get_serial_sequence('public.raspberry_pis', 'id'), COALESCE((SELECT MAX(id) FROM public.raspberry_pis), 1), true);
SELECT setval(pg_get_serial_sequence('public.settings', 'id'), COALESCE((SELECT MAX(id) FROM public.settings), 1), true);
SELECT setval(pg_get_serial_sequence('public.hour_metrics', 'id'), COALESCE((SELECT MAX(id) FROM public.hour_metrics), 1), true);
SELECT setval(pg_get_serial_sequence('public.minute_metrics', 'id'), COALESCE((SELECT MAX(id) FROM public.minute_metrics), 1), true);
SELECT setval(pg_get_serial_sequence('public.ten_second_metrics', 'id'), COALESCE((SELECT MAX(id) FROM public.ten_second_metrics), 1), true);

COMMIT;
