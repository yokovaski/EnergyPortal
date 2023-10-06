<template>
    <div>
        <div v-if="Object.keys(energyNameMapping).length > 0" class="row">
            <div v-for="(lastValue, type) in latestEnergyValues" class="col-sm-3 mb-4">
                <div class="card shadow-sm">
                    <div class="card-body">
                        <h5 class="card-text text-center">
                            <small v-bind:class="lastValue.styleClass">
                                {{ energyNameMapping[type] }}
                            </small>
                        </h5>
                        <h2 class="text-center font-weight-bold text-secondary">
                            {{ lastValue.value }}
                        </h2>
                        <h5 class="text-center text-muted"><small>kWh</small></h5>
                    </div>
                </div>
            </div>
        </div>
        <div class="row">
            <div class="col-md-8">
                <div class="row">
                    <div
                        class="col mb-4"
                        v-for="chartName in chartNames"
                        v-if="showEnergyChart(chartName)"
                    >
                        <energy-chart
                            ref="energyChart"
                            :chart-data="energyCharts[chartName].chartData"
                            :chart-name="energyCharts[chartName].chartName"
                            :active-range="chartRange"
                            v-on:change-range="setGraphRange($event)">
                        </energy-chart>
                    </div>
                </div>
            </div>
            <div class="col-md-4">
                <div class="row">
                    <div class="col-12 mb-4">
                        <div class="card shadow-sm">
                            <div class="card-body">
                                <h5 class="card-text text-center text-muted">
                                    <small>
                                        Opname vandaag
                                    </small>
                                </h5>
                                <h2 class="text-center font-weight-bold text-secondary">
                                    {{ round(energyToday.usageTotalHigh + energyToday.usageTotalLow) }}
                                </h2>
                                <h5 class="text-center text-muted"><small>kW</small></h5>
                            </div>
                        </div>
                    </div>
                    <div class="col-12 mb-4">
                        <div class="card shadow-sm">
                            <div class="card-body">
                                <h5 class="card-text text-center text-muted">
                                    <small>
                                        Teruglevering vandaag
                                    </small>
                                </h5>
                                <h2 class="text-center font-weight-bold text-secondary">
                                    {{ round(energyToday.redeliveryTotalHigh + energyToday.redeliveryTotalLow) }}
                                </h2>
                                <h5 class="text-center text-muted"><small>kW</small></h5>
                            </div>
                        </div>
                    </div>
                    <div class="col-12 mb-4">
                        <div class="card shadow-sm">
                            <div class="card-body">
                                <h5 class="card-text text-center text-muted">
                                    <small>
                                        Opwekking vandaag
                                    </small>
                                </h5>
                                <h2 class="text-center font-weight-bold text-secondary">
                                    {{ (energyToday.totalSolar) }}
                                </h2>
                                <h5 class="text-center text-muted"><small>kW</small></h5>
                            </div>
                        </div>
                    </div>
                    <div class="col-12 mb-4">
                        <div class="card shadow-sm">
                            <div class="card-body">
                                <h5 class="card-text text-center text-muted">
                                    <small>
                                        Gasverbruik vandaag
                                    </small>
                                </h5>
                                <h2 class="text-center font-weight-bold text-secondary">
                                    {{ (energyToday.totalGas) }}
                                </h2>
                                <h5 class="text-center text-muted"><small>M³</small></h5>
                            </div>
                        </div>
                    </div>
                    <div class="col-12 mb-4">
                        <div class="card shadow-sm">
                            <div class="card-body">
                                <h5 class="card-text text-center text-muted">
                                    <small>
                                        Meterstanden
                                    </small>
                                </h5>
                                <table class="table">
                                    <tr>
                                        <th class="text-muted">
                                            Opname hoog
                                        </th>
                                        <td class="text-muted">
                                            {{ totals.usageTotalHigh }}
                                        </td>
                                    </tr>
                                    <tr>
                                        <th class="text-muted">
                                            Opname laag
                                        </th>
                                        <td class="text-muted">
                                            {{ totals.usageTotalLow }}
                                        </td>
                                    </tr>
                                    <tr>
                                        <th class="text-muted">
                                            Levering hoog
                                        </th>
                                        <td class="text-muted">
                                            {{ totals.redeliveryTotalHigh }}
                                        </td>
                                    </tr>
                                    <tr>
                                        <th class="text-muted">
                                            Levering laag
                                        </th>
                                        <td class="text-muted">
                                            {{ totals.redeliveryTotalLow }}
                                        </td>
                                    </tr>
                                    <tr>
                                        <th class="text-muted">
                                            Gas
                                        </th>
                                        <td class="text-muted">
                                            {{ totals.usageGasTotal }}
                                        </td>
                                    </tr>
                                </table>
                            </div>
                        </div>
                    </div>
                </div>
            </div>
        </div>
    </div>
</template>

<script>
import EnergyChart from "./EnergyChart.vue";
import Axios from "axios"

let initialEnergyCharts = {
    usage: {
        index: 0,
        chartName: 'Verbruik',
        backEndName: 'usage',
        borderColor: '#007bff',
        backgroundColor: 'rgba(0,137,255,0.26)'
    },
    solar: {
        index: 0,
        chartName: 'Opwekking',
        backEndName: 'solar',
        borderColor: '#ffc107',
        backgroundColor: 'rgba(255,193,7,0.26)'
    },
    redelivery: {
        index: 0,
        chartName: 'Teruglevering',
        backEndName: 'redelivery',
        borderColor: '#28a745',
        backgroundColor: 'rgba(40,167,69,0.25)'
    },
    intake: {
        index: 0,
        chartName: 'Opname',
        backEndName: 'intake',
        borderColor: '#dc3545',
        backgroundColor: 'rgba(220,53,69,0.26)'
    },
};

export default {
    name: "",
    components: {EnergyChart},
    data: () => ({
        solarSystem: false,
        chartRange: 'hour',
        energyCharts: {},
        lastRefresh: null,
        latestEnergyValues: {
            usage: {
                styleClass: 'text-primary',
                value: 'N/A'
            },
            solar: {
                styleClass: 'text-warning',
                value: 'N/A'
            },
            redelivery: {
                styleClass: 'text-success',
                value: 'N/A'
            },
            intake: {
                styleClass: 'text-danger',
                value: 'N/A'
            },
        },
        energyToday: {
            usageTotalHigh: 0,
            usageTotalLow: 0,
            redeliveryTotalHigh: 0,
            redeliveryTotalLow: 0,
            totalSolar: 0,
            totalGas: 0
        },
        chartNames: [],
        energyNameMapping: {},
        totals: {
            usageTotalHigh: 0,
            redeliveryTotalHigh: 0,
            usageTotalLow: 0,
            redeliveryTotalLow: 0,
            usageGasTotal: 0,
        }
    }),
    async mounted () {
        this.initCharts();

        await Promise.all([
            this.fetchChartData(),
            this.fetchTotalsToday(),
            this.fetchTotals(),
            this.fetchUserSettings()
        ]);

        let self = this;

        setInterval(function () {
            if (self.chartRange === 'now'){
                self.fetchChartData().then();
            }

            self.fetchLast().then();
            self.fetchTotalsToday().then();
        }, 10000)
    },
    computed:{
        nowActive(){
            return this.chartRange === 'now';
        },
        hourActive() {
            return this.chartRange === 'hour';
        },
        dayActive() {
            return this.chartRange === 'day';
        },
        weekActive() {
            return this.chartRange === 'week';
        },
        chartUri() {
            if (this.weekActive)
                return '/webapi/v3/metrics/hours';

            if (this.dayActive)
                return '/webapi/v3/metrics/hours';

            if (this.hourActive)
                return '/webapi/v3/metrics/minutes';

            return '/webapi/v3/metrics/tenseconds';
        }
    },
    methods: {
        async setGraphRange(range) {
            this.chartRange = range;
            await this.fetchChartData();
        },
        async fetchLast() {
            if (this.lastRefresh === null)
                return;

            try {
                let config = {
                    params: {
                        from: this.lastRefresh
                    }
                };

                let response = await Axios.get('/webapi/v3/metrics/tenseconds', config);
                let data = response.data;

                this.lastRefresh = data.queryTimestamp;

                if (data.timestamps.length > 0) {
                    let index = data.timestamps.length - 1;

                    for (let energyType of Object.keys(this.latestEnergyValues)) {
                        this.latestEnergyValues[energyType].value = data[energyType][index];
                    }
                }
            } catch (e) {
                console.error(e);
            }
        },
        async fetchTotals() {
            try {
                let response = await Axios.get('/webapi/v3/energy/totals')
                this.totals = response.data;
            } catch (e) {
                console.error(e);
            }
        },
        async fetchTotalsToday() {
            try {
                let response = await Axios.get('/webapi/v3/energy/total-today');

                this.energyToday.usageTotalHigh = response.data.usageTotalHigh;
                this.energyToday.usageTotalLow = response.data.usageTotalLow;
                this.energyToday.redeliveryTotalHigh = response.data.redeliveryTotalHigh;
                this.energyToday.redeliveryTotalLow = response.data.redeliveryTotalLow;
                this.energyToday.totalSolar = response.data.totalSolar;
                this.energyToday.totalGas = response.data.totalGas;
            } catch (e) {
                console.error(e);
            }
        },
        async fetchUserSettings() {
            try {
                let response = await Axios.get('/webapi/v3/settings');
                this.solarSystem = response.data.solarSystem;
            } catch (e) {
                console.error(e);
                this.makeToast('Failed to fetch user settings.', 'danger');
            }
        },
        initCharts() {
            for (const chart of Object.values(initialEnergyCharts)) {
                this.energyCharts[chart.chartName] = {
                    chartName: chart.chartName,
                    chartData: this.buildChartData([], [], chart.backgroundColor, chart.borderColor),
                    backEndName: chart.backEndName
                };

                this.chartNames.push(chart.chartName);
                this.energyNameMapping[chart.backEndName] = chart.chartName;
            }
        },
        async fetchChartData() {
            try {
                let response = await Axios.get(this.chartUri, this.chartConfig());
                let data = response.data;

                for (const chartName of this.chartNames) {
                    let backEndName = this.energyCharts[chartName].backEndName;
                    let chartData = data[backEndName];

                    if (chartData === undefined)
                        continue;

                    if (chartData.length > 0 && this.lastRefresh === null)
                        this.latestEnergyValues[backEndName].value = chartData[chartData.length -1];

                    this.energyCharts[chartName].chartData.datasets[0].data = chartData;
                    this.energyCharts[chartName].chartData.labels = data.timestamps;
                }

                this.lastRefresh = data.queryTimestamp;

                if (!this.$refs.energyChart) {
                    return;
                }

                for (let i = 0; i < this.$refs.energyChart.length; i++){
                    this.$refs.energyChart[i].refresh();
                }

            } catch (e) {
                console.error(e);
            }
        },
        buildChartData(data, labels, backgroundColor, borderColor) {
            return {
                datasets: [
                    {
                        backgroundColor: backgroundColor,
                        borderColor: borderColor,
                        data: data
                    }
                ],
                labels: labels
            }
        },
        round(value, digits = 3) {
            let ePlus = `e+${digits}`;
            let eMinus = `e-${digits}`;

            return +(Math.round(value + ePlus) + eMinus);
        },
        makeToast(message, variant = null) {
            this.$bvToast.toast(message, {
                variant: variant,
                solid: true
            })
        },
        showEnergyChart(chartName) {
            if (this.solarSystem)
                return true;

            return chartName === initialEnergyCharts.usage.chartName;
        },
        chartConfig() {
            let now = new Date();

            if (this.weekActive)
                now.setHours(now.getHours() - 168);

            if (this.dayActive)
                now.setHours(now.getHours() - 24);

            if (this.hourActive)
                now.setMinutes(now.getMinutes() - 60);

            if (this.nowActive)
                now.setMinutes(now.getMinutes() - 10);

            return {
                params: {
                    from: now.toISOString()
                }
            };
        }
    }
}
</script>

<style scoped>

</style>