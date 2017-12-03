Vue.component('thumbnail', {
    template: `
        <ul>
            <li v-for="report in reports">
                <img src="http://192.168.43.46:8000/api/crime/downloadCrime/{{ report.file_name }}" width="100%" height="345px"/>
            </li>
        </ul>
    `
});

new Vue({
    el: '#root',
    data () {
        return {
            reports: [],
        };
    }
});