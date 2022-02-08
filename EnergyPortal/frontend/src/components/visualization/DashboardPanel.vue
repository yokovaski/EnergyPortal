<template>
    <v-row>
        <panel-card 
            v-for="(card, i) in cards"
            :card="card"
            :key="i"
        ></panel-card>
    </v-row>
</template>

<script>
import PanelCard from "@/components/visualization/PanelCard";
export default {
    name: "DashboardPanel",
    components: {PanelCard},
    props: {
        panel: {
            required: true,
            type: Object
        }
    },
    data: () => ({
        cards: []
    }),
    created() {
        this.cards = this.panel.cards;
    }
}
</script>

<style scoped>

</style>