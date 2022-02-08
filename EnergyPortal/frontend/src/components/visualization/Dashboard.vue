<template>
    <div class="pa-6">
        <!--    Header here-->
        <dashboard-panel
            v-for="(panel, i) in panels"
            :panel="panel"
            :key="i"
        ></dashboard-panel>
    </div>
</template>

<script>
import DashboardPanel from "@/components/visualization/DashboardPanel";
export default {
    name: "Dashboard",
    components: {DashboardPanel},
    data: () => ({
        panels: [
            {
                cards: [
                    {
                        width: 3,
                        text: "Hi"
                    },
                    {
                        width: 4,
                        text: "Hello"
                    },
                    {
                        width: 2,
                        text: "Hi"
                    },
                    {
                        width: 4,
                        text: "Hello"
                    },
                    {
                        width: 2,
                        text: "Hi"
                    },
                    {
                        width: 4,
                        text: "Hello"
                    },
                    {
                        width: 2,
                        text: "Hi"
                    },
                    {
                        width: 4,
                        text: "Hello"
                    },
                ]
            }
        ]
    })
}
</script>

<style scoped>

</style>