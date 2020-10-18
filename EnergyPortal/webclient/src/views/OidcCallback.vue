<template>
  <p>Redirecting...</p>
</template>

<script>
import { mapActions } from "vuex";

export default {
  name: "OidcCallback",
  methods: {
    ...mapActions(["oidcSignInCallback"])
  },
  mounted() {
    this.oidcSignInCallback()
      .then(redirectPath => {
        this.$router.push(redirectPath);
      })
      .catch(err => {
        console.error(err);
        this.$router.push("/oidc-callback-error");
      });
  }
};
</script>

<style scoped></style>
