<script>
    import { Line, mixins } from 'vue-chartjs';
    import Chart from 'chart.js';

    Chart.scaleService.updateScaleDefaults('linear', {
        ticks: {
            min: 0,
            precision: 1,
            beginAtZero: true,
            suggestedMax: 1
        }
    });

    export default {
        extends: Line,
        mixins: [mixins.reactiveProp],
        props: ['options'],
        mounted () {
            this.renderChart(this.chartData, this.options);
        },
        methods: {
            refresh() {
                this.$data._chart.update();
            }
        }
    };
</script>

<style scoped>

</style>