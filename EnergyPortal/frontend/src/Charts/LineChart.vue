<script>
import { Line, mixins } from 'vue-chartjs';

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