Vue.component('description', {
    template: `
        <div>
            <div>
                <label id="label">description:</label>
            </div>
            <div>
                <textarea rows="4" cols="85%">
            At w3schools.com you will learn how to make a website. We offer free tutorials in all web development technologies.
                </textarea>
            </div>
            <ul>
                <li v-for="rep in this.reports">asdasdas</li>
            </ul>
        </div>
    `
});

new Vue({
    el: '#root'
});