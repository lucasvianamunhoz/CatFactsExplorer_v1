import { createRouter, createWebHistory } from "vue-router";
 
import CatFactsGrid from "@/components/CatFactsGrid.vue";

const routes = [
    {
        path: "/",
        name: "Home",
        component: CatFactsGrid,
    },
];

const router = createRouter({
    history: createWebHistory(),
    routes,
});

export default router;