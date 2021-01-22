<template>
  <div class="card h-100 shadow-sm">
    <div class="card-body">
      <h6 class="card-title text-muted text-center">{{ chartName }}</h6>
      <line-chart class="energy-chart" ref="lineChart"
                  :chart-data="chartData"
                  :options="options">
      </line-chart>
      <div class="text-center chart-range-group">
        <div class="btn-group btn-group-sm" role="group" aria-label="...">
          <button type="button"
                  class="chartRangeSelector btn-hours btn btn-sm btn-outline-secondary"
                  onclick="this.blur();"
                  v-on:click.prevent="$emit('change-range', 'now')"
                  v-bind:class="{ active: nowActive }">
            Nu
          </button>
          <button type="button"
                  class="chartRangeSelector btn-hours btn btn-sm btn-outline-secondary"
                  onclick="this.blur();"
                  v-on:click.prevent="$emit('change-range', 'hour')"
                  v-bind:class="{ active: hourActive }">
            Uur
          </button>
          <button type="button"
                  class="chartRangeSelector btn-days btn btn-sm btn-outline-secondary"
                  onclick="this.blur();"
                  v-on:click.prevent="$emit('change-range', 'day')"
                  v-bind:class="{ active: dayActive }">
            Dag
          </button>
          <button type="button"
                  class="chartRangeSelector btn-months btn btn-sm btn-outline-secondary"
                  onclick="this.blur();"
                  v-on:click.prevent="$emit('change-range', 'week')"
                  v-bind:class="{ active: weekActive }">
            Week
          </button>
        </div>
      </div>
    </div>
  </div>
</template>

<script>
import LineChart from '../Charts/LineChart';

export default {
  name: 'EnergyChart',
  components: { LineChart },
  props: ['chartName', 'chartData', 'activeRange'],
  data: function() {
    return {
      options: {
        legend: {
          display: false
        },
        maintainAspectRatio: false,
        scales:{
          xAxes: [{
            ticks: {
              autoSkip: true,
              maxTicksLimit: 10
            }
          }],
          yAxes: [{
            ticks: {
              beginAtZero: true,
              suggestedMax: 1
            }
          }]
        }
      }
    }
  },
  computed:{
    nowActive() {
      return this.activeRange === 'now';
    },
    hourActive() {
      return this.activeRange === 'hour';
    },
    dayActive() {
      return this.activeRange === 'day';
    },
    weekActive() {
      return this.activeRange === 'week';
    },
  },
  methods: {
    refresh() {
      this.$refs.lineChart.refresh();
    }
  },
  watch: {
    chartData: {
      deep: true,
      handler () {
        console.log('detected change in EnergyChart');
      }
    },
    // activeRange() {
    //     console.log('Active range changed');
    //     this.$refs.lineChart.refresh();
    // }
  }
};
</script>

<style scoped>
.energy-chart {
  height: 250px;
}

.chart-range-group {
  margin-top: 0.5rem;
}
</style>