<template>
    <v-col
        :cols="card.width"
    >
        <v-card
            elevation="3"
            :style="style"
        >
            <div
                class="text-h4 text-center text--secondary"
                v-text="card.text"
            ></div>
        </v-card>
    </v-col>
</template>

<script>
export default {
    name: "PanelCard",
    props: {
        card: {
            required: true,
            type: Object
        }
    },
    data: () => ({
        height: 5
    }),
    computed: {
        actualHeight() {
            return this.height * 20;
        },
        style() {
            return {
                height: `${this.actualHeight}px`
            };
        }
    }
}
</script>

<style scoped>

</style>