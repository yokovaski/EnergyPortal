<template>
    <v-container>
        <home-dashboard :key="Date.now()"></home-dashboard>
    </v-container>
</template>

<script>
// @ is an alias to /src
import HomeDashboard from "@/components/HomeDashboard";
export default {
    name: "Home",
    components: {HomeDashboard}
};
</script>