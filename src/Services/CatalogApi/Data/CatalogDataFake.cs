using CatalogApi.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CatalogApi.Data
{
    public class CatalogDataFake
    {
        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        private static List<CatalogItem> _itemsList;
        private static IEnumerable<CatalogTree> _treeArray;

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        public async Task Refresh(bool bForce = false)
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
        {
            var rng = new Random();
            if (_itemsList == null || bForce)
            {
                _itemsList = Enumerable.Range(1, 12).Select(index => new CatalogItem
                {
                    Id = index,
                    CatalogId = index % 5 * 10 + index % 3,
                    Name = Summaries[rng.Next(Summaries.Length)],
                    Description = "Desc:" + Summaries[rng.Next(Summaries.Length)],
                    Price = rng.Next(20, 150),
                    PictureUri = index.ToString() + ".png"
                }).ToList();

            }

            if (_treeArray == null || bForce)
            {
                _treeArray = Enumerable.Range(1, 5).Select(index => new CatalogTree
                {
                    id = index * 10,
                    idp = 0,
                    n = Summaries[rng.Next(Summaries.Length)],
                    s = Enumerable.Range(1, 3).Select(indexchild => new CatalogTree
                    {

                        id = index * 10 + indexchild,
                        idp = index * 10,
                        n = Summaries[rng.Next(Summaries.Length)],

                    })
                        .ToList()
                })
                .ToArray();
            }
        }

        public IEnumerable<CatalogTree> GetTreeArray()
        {
            return _treeArray;
        }

        public List<CatalogItem> GetItemsList()
        {
            return _itemsList;
        }

        public async Task<List<CatalogItem>> GetItemsByCatalogIdsAsync(int catalogId = 0)
        {
            string ids = GetItemsIdByCatalogIds(catalogId);
            var numIds = ids.Split(',').Select(id => (Ok: int.TryParse(id, out int x), Value: x));
            if (!numIds.All(nid => nid.Ok))
            {
                return new List<CatalogItem>();
            }

            var idsToSelect = numIds
                .Select(id => id.Value);

            var items = await Task.Run(() => { return _itemsList.Where(ci => idsToSelect.Contains(ci.CatalogId)).ToList(); });

            return items;
        }
        public async Task<List<CatalogItem>> GetItemsByIdsAsync(string ids)
        {
            var numIds = ids.Split(',').Select(id => (Ok: int.TryParse(id, out int x), Value: x));

            if (!numIds.All(nid => nid.Ok))
            {
                return new List<CatalogItem>();
            }

            var idsToSelect = numIds
                .Select(id => id.Value);

            var items = await Task.Run(() => { return _itemsList.Where(ci => idsToSelect.Contains(ci.Id)).ToList(); });

            return items;
        }

        private string GetSubfulderIds(string strids, CatalogTree treeitem)
        {
            strids += treeitem.id + ",";
            if (treeitem.s != null)
                foreach (var subtree in treeitem.s)
                {
                    strids = GetSubfulderIds(strids, subtree);
                }

            return strids;
        }
        public string GetItemsIdByCatalogIds(int catalogId)
        {
            //var items = await Task<List<CatalogItem>>.Run(() => { return _itemsList.Where(ci => (ci.CatalogId == catalogId)).ToList(); });
            string strids = "";
            bool bNotFound = true;
            foreach (var treeitem in _treeArray)
            {
                strids = "";
                strids += GetSubfulderIds(strids, treeitem);
                int index = strids.IndexOf(catalogId.ToString());
                if (index >= 0)
                {
                    bNotFound = false;
                    strids = strids.Substring(index);
                    break;
                }

            }

            return bNotFound ? "" : strids.Substring(0, strids.Length - 1);

        }

    }
}
