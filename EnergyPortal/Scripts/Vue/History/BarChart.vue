<script>
    import { Bar, mixins } from 'vue-chartjs';
    
    export default {
        name: "BarChart",
        extends: Bar,
        props: {
            chartData: {
                type: Object,
                default: null
            },
            options: {
                type: Object,
                default: null
            }
        },
        mounted() {
            this.renderChart(this.chartData, this.options);
        },
        methods: {
            update() {
                this.$data._chart.update();
            }
        }
    }
</script>

<style scoped>

</style>