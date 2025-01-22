<template>
    <div>
        <h1>Cat Facts</h1>
        <input v-model="filter" placeholder="Filter facts..." />
        <table>
            <thead>
                <tr>
                    <th>Fact</th>
                    <th>Length</th>
                </tr>
            </thead>
            <tbody>
                <tr v-for="fact in filteredCatFacts" :key="fact.fact">
                    <td>{{ fact.fact }}</td>
                    <td>{{ fact.length }}</td>
                </tr>
            </tbody>
        </table>
    </div>
</template>

<script lang="ts">
    import { defineComponent, ref, computed, onMounted, Ref } from "vue";
    import { getCatFacts } from "@/services/api";

    // Define a interface do fato de gato
    interface CatFact {
        fact: string;
        length: number;
    }

    export default defineComponent({
        name: "CatFactsGrid",
        setup() {
            // Estado reativo que guarda a lista de fatos
            const catFacts: Ref<CatFact[]> = ref([]);

            // Estado reativo para o campo de filtro
            const filter = ref("");

            // Busca os fatos da API
            const fetchCatFacts = async () => {
                catFacts.value = await getCatFacts();
            };

            // Computed para filtrar os fatos com base no campo "filter"
            const filteredCatFacts = computed(() =>
                catFacts.value.filter((catFact) =>
                    catFact.fact.toLowerCase().includes(filter.value.toLowerCase())
                )
            );

            // Faz a busca logo que o componente é montado
            onMounted(() => {
                fetchCatFacts();
            });

            // Expondo propriedades e computeds pro template
            return { catFacts, filter, filteredCatFacts };
        },
    });
</script>

<style scoped>
    table {
        width: 100%;
        border-collapse: collapse;
    }

    th,
    td {
        padding: 8px;
        text-align: left;
        border: 1px solid #ddd;
    }

    input {
        margin-bottom: 16px;
        padding: 8px;
        width: 100%;
    }
</style>